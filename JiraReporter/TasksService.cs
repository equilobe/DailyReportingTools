using JiraReporter.Model;
using SourceControlLogReporter;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    class TasksService
    {
        public List<Issue> GetCompletedTasks(Policy policy, Options options, Timesheet timesheet)
        {
            var completedTasks = new List<Issue>();
            var issues = RestApiRequests.GetCompletedIssues(policy, DateTime.Now.ToOriginalTimeZone().AddDays(-6), DateTime.Now.ToOriginalTimeZone());
            foreach (var issue in issues.issues)
            {
                if (issue.fields.issuetype.subtask == false)
                    SetTask(policy, issue, timesheet, completedTasks, null);
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks; 
        }

        public AnotherJiraRestClient.Issues GetUnfinishedTasks (Policy policy)
        {
            return RestApiRequests.GetSprintTasks(policy);
        }

        public void SetUnfinishedTasks(AnotherJiraRestClient.Issues jiraIssues, SprintTasks tasks, Timesheet timesheet, List<PullRequest> pullRequests, Policy policy)
        {
            tasks.InProgressTasks = new UncompletedTasks();
            tasks.InProgressTasks.Issues = new List<Issue>();
            tasks.OpenTasks = new UncompletedTasks();
            tasks.OpenTasks.Issues = new List<Issue>();
            tasks.UnassignedTasks = new UncompletedTasks();
            tasks.UnassignedTasks.Issues = new List<Issue>();
            tasks.UncompletedTasks = new List<Issue>();

            foreach (var jiraIssue in jiraIssues.issues)
            {
                var issue = new Issue
                {
                    Key = jiraIssue.key,
                    Summary = jiraIssue.fields.summary
                };
                IssueAdapter.SetIssue(issue, policy, jiraIssue, timesheet, pullRequests);
                tasks.UncompletedTasks.Add(issue);
                if (issue.StatusCategory.name == "In Progress")
                    tasks.InProgressTasks.Issues.Add(issue);
                else
                    if (issue.Resolution == null)
                        tasks.OpenTasks.Issues.Add(issue);
                if (issue.Assignee == null && issue.StatusCategory.name != "Done")
                    tasks.UnassignedTasks.Issues.Add(issue);
            }
        }

        public void SetTask(SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue jiraIssue, Timesheet timesheet, List<Issue> tasks, List<PullRequest> pullRequests)
        {
            tasks.Add(new Issue
            {
                Key = jiraIssue.key,
                Summary = jiraIssue.fields.summary
            });

            IssueAdapter.SetIssue(tasks.Last(), policy, jiraIssue, timesheet, pullRequests);
        }

        public IEnumerable<IGrouping<string, Issue>> GroupCompletedTasks(List<Issue> completedTasks)
        {
            var tasks = from task in completedTasks
                        group task by task.CompletedTimeAgo into newGroup
                        orderby newGroup.Min(g => g.ResolutionDate.ToOriginalTimeZone())
                        select newGroup;
            tasks = tasks.OrderByDescending(t => t.Min(g => g.ResolutionDate));
            return tasks;
        }

        public void SetCompletedTasks(IEnumerable<IGrouping<string, Issue>> tasks, SprintTasks sprintTasks)
        {
            var completedTasks = new Dictionary<string, List<Issue>>();
            var issues = new List<Issue>();
            foreach (var task in tasks)
            {
                issues = tasks.SelectMany(group => group).Where(group => group.CompletedTimeAgo == task.Key).ToList();
                completedTasks.Add(task.Key, issues);
            }
            sprintTasks.CompletedTasks = completedTasks;
        }

        public void SortTasks(SprintTasks sprintTasks)
        {
            if (sprintTasks.InProgressTasks != null)
                sprintTasks.InProgressTasks.Issues = sprintTasks.InProgressTasks.Issues.OrderBy(priority => priority.Priority.id).ToList();
            if (sprintTasks.OpenTasks != null)
                sprintTasks.OpenTasks.Issues = sprintTasks.OpenTasks.Issues.OrderBy(priority => priority.Priority.id).ToList();
            if (sprintTasks.UnassignedTasks != null)
                sprintTasks.UnassignedTasks.Issues = sprintTasks.UnassignedTasks.Issues.OrderBy(priority => priority.Priority.id).ToList();
        }

        public bool HasOpenTasks(Issue task)
        {
            return task.SubtasksIssues.Exists(i => i.StatusCategory.name != "In Progress" && i.Resolution == null);
        }

        public bool HasInProgress(Issue task)
        {
            return task.SubtasksIssues.Exists(i => i.StatusCategory.name == "In Progress");
        }

        public bool HasUnassigned(Issue task)
        {
            return task.SubtasksIssues.Exists(i => i.Resolution == null && i.Assignee == null);
        }
    }
}
