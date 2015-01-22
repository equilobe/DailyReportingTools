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
        public List<Issue> GetCompletedTasks(Policy policy, Options options)
        {
            var completedTasks = new List<Issue>();
            var issues = RestApiRequests.GetCompletedIssues(policy, DateTime.Now.ToOriginalTimeZone().AddDays(-6), DateTime.Now.ToOriginalTimeZone());
            foreach (var issue in issues.issues)
            {
                if (issue.fields.issuetype.subtask == false)
                    SetTask(policy, issue, completedTasks, null);
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks;
        }

        public AnotherJiraRestClient.Issues GetUnfinishedTasks(Policy policy)
        {
            return RestApiRequests.GetSprintTasks(policy);
        }

        public void SetUnfinishedTasks(AnotherJiraRestClient.Issues jiraIssues, SprintTasks tasks, List<PullRequest> pullRequests, Policy policy)
        {
            tasks.InProgressTasks = new List<Issue>();
            tasks.OpenTasks = new List<Issue>();
            tasks.UnassignedTasks = new List<Issue>();

            foreach (var jiraIssue in jiraIssues.issues)
            {
                var issue = new Issue
                {
                    Key = jiraIssue.key,
                    Summary = jiraIssue.fields.summary
                };
                IssueAdapter.SetIssue(issue, policy, jiraIssue, pullRequests); 
                IssueAdapter.SetIssueErrors(issue, policy);

                if (issue.StatusCategory.name == "In Progress")
                    tasks.InProgressTasks.Add(issue);
                else
                    if (issue.Resolution == null)
                        tasks.OpenTasks.Add(issue);
                if (issue.Assignee == null && issue.StatusCategory.name != "Done")
                    tasks.UnassignedTasks.Add(issue);
            }
        }

        public void SetTask(SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue jiraIssue, List<Issue> tasks, List<PullRequest> pullRequests)
        {
            tasks.Add(new Issue
            {
                Key = jiraIssue.key,
                Summary = jiraIssue.fields.summary
            });

            IssueAdapter.SetIssue(tasks.Last(), policy, jiraIssue, pullRequests);
            IssueAdapter.SetIssueErrors(tasks.Last(), policy);
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

        public static List<Issue> GetParentTasks(List<Issue> tasks, JiraReporter.Model.Author author)
        {
            List<Issue> parentTasks = new List<Issue>(tasks);
            foreach (var task in tasks)
            {
                IssueAdapter.SetSubtasksLoggedAuthor(task, author.Name);
                if (task.SubTask == true)
                {
                    IssueAdapter.SetLoggedAuthor(task, author.Name);
                    var parentIssue = parentTasks.Find(t => t.Key == task.Parent.Key);
                    var parent = new Issue();
                    if (parentIssue != null)
                    {
                        parent = new Issue(parentIssue);
                        if (parent.AssigneeSubtasks == null)
                            parent.AssigneeSubtasks = new List<Issue>();
                        if (parent.AssigneeSubtasks.Exists(t => t.Key == task.Key) == false)
                            parent.AssigneeSubtasks.Add(new Issue(task));
                    }
                    else
                    {
                        parent = CreateParent(task, author);
                        parent.AssigneeSubtasks.Add(new Issue(task));
                        IssueAdapter.SetLoggedAuthor(parent, author.Name);
                        parentTasks.Add(parent);
                    }
                }
            }
            parentTasks.RemoveAll(t => t.SubTask == true);
            return parentTasks;
        }

        private static Issue CreateParent(Issue task, JiraReporter.Model.Author author)
        {
            var parent = new Issue(task.Parent);
            foreach (var subtask in parent.SubtasksIssues)
                IssueAdapter.SetLoggedAuthor(subtask, author.Name);
            parent.AssigneeSubtasks = new List<Issue>();
            return parent;
        }

        public static void SetErrors(List<Issue> tasks, Policy policy)
        {
            if (tasks != null && tasks.Count > 0)
                foreach (var task in tasks)
                    IssueAdapter.SetIssueErrors(task, policy);
        }

        public static int GetErrorsCount(List<Issue> tasks)
        {
            if (tasks != null)
                return tasks.Sum(t => t.ErrorsCount);
            else
                return 0;
        }

        public static int GetTimeLeftForSpecificAuthorTasks(List<Issue> tasks, string authorName)
        {
            return tasks.Where(t => t.Assignee == authorName).Sum(i => i.TotalRemainingSeconds);
        }
    }
}
