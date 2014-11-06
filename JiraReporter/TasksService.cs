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
            var issues = RestApiRequests.GetCompletedIssues(policy, DateTime.Today.AddDays(-6).ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone());
            foreach (var issue in issues.issues)
            {
                if (issue.fields.issuetype.subtask == false)
                    SetTasks(policy, issue, timesheet, completedTasks, null);
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks; 
        }

        public AnotherJiraRestClient.Issues GetUnfinishedTasks (Policy policy)
        {
            return RestApiRequests.GetSprintTasks(policy);
        }

        public void SetUnfinishedTasks(AnotherJiraRestClient.Issues issues, SprintTasks tasks, Timesheet timesheet, List<PullRequest> pullRequests, Policy policy)
        {
            tasks.InProgressTasks = new List<Issue>();
            tasks.OpenTasks = new List<Issue>();
            tasks.UnassignedTasks = new List<Issue>();
            foreach (var issue in issues.issues)
            {
                if (issue.fields.status.statusCategory.name == "In Progress")
                    SetTasks(policy, issue, timesheet, tasks.InProgressTasks, pullRequests);
                else
                    if (issue.fields.resolution == null)
                        SetTasks(policy, issue, timesheet, tasks.OpenTasks, pullRequests);
                if (issue.fields.assignee == null && issue.fields.status.statusCategory.name != "Done")
                    SetTasks(policy, issue, timesheet, tasks.UnassignedTasks, pullRequests);
            }
        }

        public void SetTasks(SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Issue> tasks, List<PullRequest> pullRequests)
        {
            tasks.Add(new Issue
            {
                Key = issue.key,
                Summary = issue.fields.summary
            });

            IssueAdapter.SetIssue(tasks.Last(), policy, issue, timesheet, pullRequests);
            if (tasks.Last().SubTask == true)
                IssueAdapter.SetParent(tasks.Last(), issue, policy, timesheet, pullRequests);
            if (tasks.Last().Subtasks != null)
                IssueAdapter.SetSubtasksIssues(tasks.Last(), policy, timesheet, pullRequests);
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
                sprintTasks.InProgressTasks = sprintTasks.InProgressTasks.OrderBy(priority => priority.Priority.id).ToList();
            if (sprintTasks.OpenTasks != null)
                sprintTasks.OpenTasks = sprintTasks.OpenTasks.OrderBy(priority => priority.Priority.id).ToList();
            if (sprintTasks.UnassignedTasks != null)
                sprintTasks.UnassignedTasks = sprintTasks.UnassignedTasks.OrderBy(priority => priority.Priority.id).ToList();
        }
    }
}
