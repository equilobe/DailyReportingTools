using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    class TasksService
    {
        public void SetSprintTasks(JiraReport context)
        {
            context.SprintTasks = new SprintTasks();
            var issues = RestApiRequests.GetSprintTasks(context.Policy);
            var unfinishedTasks = GetUnfinishedTasks(context.Policy);
            SetUnfinishedTasks(unfinishedTasks, context);

            var completedTasks = GetCompletedTasks(context);
            SetCompletedTasks(GroupCompletedTasks(completedTasks, context), context.SprintTasks);
            SortTasks(context.SprintTasks);
            SetSprintTasksErrors(context);
        }

        private void SetSprintTasksErrors(JiraReport report)
        {
            int completedErrors = 0;
            TasksService.SetErrors(report.SprintTasks.UnassignedTasks, report.Policy);
            foreach (var list in report.SprintTasks.CompletedTasks)
            {
                TasksService.SetErrors(list.Value, report.Policy);
                completedErrors += TasksService.GetErrorsCount(list.Value);
            }
            report.SprintTasks.CompletedTasksErrorCount = completedErrors;
            report.SprintTasks.UnassignedTasksErrorCount = TasksService.GetErrorsCount(report.SprintTasks.UnassignedTasks);
        } 

        public List<Issue> GetCompletedTasks(JiraReport context)
        {
            var completedTasks = new List<Issue>();
            var issues = RestApiRequests.GetCompletedIssues(context.Policy, context.ReportDate.AddDays(-6), context.ReportDate);
            foreach (var jiraIssue in issues.issues)
            {
                if (jiraIssue.fields.issuetype.subtask == false)
                {
                    var issue = GetCompleteIssue(context, jiraIssue);

                    completedTasks.Add(issue);
                }
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks;
        }

        public JiraIssues GetUnfinishedTasks(JiraPolicy policy)
        {
            return RestApiRequests.GetSprintTasks(policy);
        }

        public void SetUnfinishedTasks(JiraIssues jiraIssues, JiraReport context)
        {
            var tasks = context.SprintTasks;
            tasks.InProgressTasks = new List<Issue>();
            tasks.OpenTasks = new List<Issue>();
            tasks.UnassignedTasks = new List<Issue>();

            foreach (var jiraIssue in jiraIssues.issues)
            {
                var issue = GetCompleteIssue(context, jiraIssue);

                if (issue.StatusCategory.name == "In Progress")
                    tasks.InProgressTasks.Add(issue);
                else
                    if (issue.Resolution == null)
                    {
                        IssueAdapter.HasSubtasksInProgress(issue);
                        tasks.OpenTasks.Add(issue);
                    }
                if (issue.Assignee == null && issue.StatusCategory.name != "Done")
                    tasks.UnassignedTasks.Add(issue);
            }
        }

        private static Issue GetCompleteIssue(JiraReport context, JiraIssue jiraIssue)
        {
            var issue = new Issue(jiraIssue);
            var issueProcessor = new IssueProcessor(context);

            issueProcessor.SetIssue(issue, jiraIssue);
            IssueAdapter.SetIssueErrors(issue, context.Policy);
            return issue;
        }

        public IEnumerable<IGrouping<string, Issue>> GroupCompletedTasks(List<Issue> completedTasks, JiraReport context)
        {
            var tasks = from task in completedTasks
                        group task by task.CompletedTimeAgo into newGroup
                        orderby newGroup.Min(g => g.ResolutionDate.ToOriginalTimeZone(context.OffsetFromUtc))
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

        public static List<Issue> GetParentTasks(List<Issue> tasks, JiraAuthor author)
        {
            List<Issue> parentTasks = new List<Issue>(tasks);
            foreach (var task in tasks)
            {
                IssueAdapter.SetSubtasksLoggedAuthor(task, author.Name);
                if (task.IsSubtask == true)
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
            parentTasks.RemoveAll(t => t.IsSubtask == true);
            return parentTasks;
        }

        private static Issue CreateParent(Issue task, JiraAuthor author)
        {
            var parent = new Issue(task.Parent);
            foreach (var subtask in parent.SubtasksIssues)
                IssueAdapter.SetLoggedAuthor(subtask, author.Name);
            parent.AssigneeSubtasks = new List<Issue>();
            return parent;
        }

        public static void SetErrors(List<Issue> tasks, JiraPolicy policy)
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
