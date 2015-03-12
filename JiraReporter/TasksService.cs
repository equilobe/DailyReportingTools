using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using JiraReporter.Model;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Equilobe.DailyReport.Models.Policy;

namespace JiraReporter
{
    class TasksService
    {
        public void SetReportTasks(JiraReport context)
        {
            context.ReportTasks = new SprintTasks();
            if(context.HasSprint)
            {
                var unfinishedTasks = GetUnfinishedTasks(context);
                SetUnfinishedTasks(unfinishedTasks, context);
                context.ReportTasks.UncompletedTasks = context.ReportTasks.InProgressTasks.Concat(context.ReportTasks.OpenTasks).ToList();
                SortTasks(context.ReportTasks);
                SetUnassignedTasksErrors(context);
            }
           
            var completedTasks = GetCompletedTasks(context);
            SetCompletedTasks(GroupCompletedTasks(completedTasks, context), context.ReportTasks);
            SetCompletedTasksErrors(context);
        }

        private void SetCompletedTasksErrors(JiraReport report)
        {
            int completedErrors = 0;
            foreach (var list in report.ReportTasks.CompletedTasks)
            {
                TasksService.SetErrors(list.Value, report.Policy);
                completedErrors += TasksService.GetErrorsCount(list.Value);
            }
            report.ReportTasks.CompletedTasksErrorCount = completedErrors;
        }

        private static void SetUnassignedTasksErrors(JiraReport report)
        {
            TasksService.SetErrors(report.ReportTasks.UnassignedTasks, report.Policy);
            report.ReportTasks.UnassignedTasksErrorCount = TasksService.GetErrorsCount(report.ReportTasks.UnassignedTasks);
        } 

        public List<IssueDetailed> GetCompletedTasks(JiraReport context)
        {
            var completedTasks = new List<IssueDetailed>();
            var issues = new JiraService(context.JiraRequestContext).GetCompletedIssues(context.ProjectKey, context.ReportDate.AddDays(-6), context.ReportDate);
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

        public JiraIssues GetUnfinishedTasks(JiraReport context)
        {
            return new JiraService(context.JiraRequestContext).GetSprintTasks(context.ProjectKey);
        }

        public void SetUnfinishedTasks(JiraIssues jiraIssues, JiraReport context)
        {
            var tasks = context.ReportTasks;
            tasks.InProgressTasks = new List<IssueDetailed>();
            tasks.OpenTasks = new List<IssueDetailed>();
            tasks.UnassignedTasks = new List<IssueDetailed>();

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

        private static IssueDetailed GetCompleteIssue(JiraReport context, JiraIssue jiraIssue)
        {
            var issue = new IssueDetailed(jiraIssue);
            var issueProcessor = new IssueProcessor(context);

            issueProcessor.SetIssue(issue, jiraIssue);
            IssueAdapter.SetIssueErrors(issue, context.Policy);
            return issue;
        }

        public IEnumerable<IGrouping<string, IssueDetailed>> GroupCompletedTasks(List<IssueDetailed> completedTasks, JiraReport context)
        {
            var tasks = from task in completedTasks
                        group task by task.CompletedTimeAgo into newGroup
                        orderby newGroup.Min(g => g.ResolutionDate.ToOriginalTimeZone(context.OffsetFromUtc))
                        select newGroup;
            tasks = tasks.OrderByDescending(t => t.Min(g => g.ResolutionDate));
            return tasks;
        }

        public void SetCompletedTasks(IEnumerable<IGrouping<string, IssueDetailed>> tasks, SprintTasks sprintTasks)
        {
            var completedTasks = new Dictionary<string, List<IssueDetailed>>();
            var issues = new List<IssueDetailed>();
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

        public static List<IssueDetailed> GetParentTasks(List<IssueDetailed> tasks, JiraAuthor author)
        {
            List<IssueDetailed> parentTasks = new List<IssueDetailed>(tasks);
            foreach (var task in tasks)
            {
                IssueAdapter.SetSubtasksLoggedAuthor(task, author.Name);
                if (task.IsSubtask == true)
                {
                    IssueAdapter.SetLoggedAuthor(task, author.Name);
                    var parentIssue = parentTasks.Find(t => t.Key == task.Parent.Key);
                    var parent = new IssueDetailed();
                    if (parentIssue != null)
                    {
                        parent = new IssueDetailed(parentIssue);
                        if (parent.AssigneeSubtasks == null)
                            parent.AssigneeSubtasks = new List<IssueDetailed>();
                        if (parent.AssigneeSubtasks.Exists(t => t.Key == task.Key) == false)
                            parent.AssigneeSubtasks.Add(new IssueDetailed(task));
                    }
                    else
                    {
                        parent = CreateParent(task, author);
                        parent.AssigneeSubtasks.Add(new IssueDetailed(task));
                        IssueAdapter.SetLoggedAuthor(parent, author.Name);
                        parentTasks.Add(parent);
                    }
                }
            }
            parentTasks.RemoveAll(t => t.IsSubtask == true);
            return parentTasks;
        }

        private static IssueDetailed CreateParent(IssueDetailed task, JiraAuthor author)
        {
            var parent = new IssueDetailed(task.Parent);
            foreach (var subtask in parent.SubtasksIssues)
                IssueAdapter.SetLoggedAuthor(subtask, author.Name);
            parent.AssigneeSubtasks = new List<IssueDetailed>();
            return parent;
        }

        public static void SetErrors(List<IssueDetailed> tasks, JiraPolicy policy)
        {
            if (tasks != null && tasks.Count > 0)
                foreach (var task in tasks)
                    IssueAdapter.SetIssueErrors(task, policy);
        }

        public static int GetErrorsCount(List<IssueDetailed> tasks)
        {
            if (tasks != null)
                return tasks.Sum(t => t.ErrorsCount);
            else
                return 0;
        }

        public static int GetTimeLeftForSpecificAuthorTasks(List<IssueDetailed> tasks, string authorName)
        {
            return tasks.Where(t => t.Assignee == authorName).Sum(i => i.TotalRemainingSeconds);
        }
    }
}
