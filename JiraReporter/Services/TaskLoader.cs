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
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.BL;
using Equilobe.DailyReport.Utils;

namespace JiraReporter.Services
{
    class TaskLoader
    {
        public IJiraService JiraService { get; set; }

        public void SetReportTasks(JiraReport context)
        {
            context.ReportTasks = new ReportTasks();
            if (context.HasSprint)
            {
                SetSprintDetails(context);
            }

            context.ReportTasks.CompletedTasksAll = GetCompletedTasks(context);
            SetVisibleCompletedTasks(context);
            SetCompletedTasksErrors(context);
        }

        private void SetSprintDetails(JiraReport context)
        {
            var unfinishedTasks = GetSprintTasks(context, context.Sprint.Id.ToString());
            SetUnfinishedTasks(unfinishedTasks, context);
            context.ReportTasks.FutureSprintTasks = GetAditionalSprintIssues(context, context.FutureSprint);
            context.ReportTasks.PastSprintTasks = GetAditionalSprintIssues(context, context.PastSprint);
            context.ReportTasks.UncompletedTasks = context.ReportTasks.InProgressTasks.Concat(context.ReportTasks.OpenTasks).ToList();
            SortTasks(context);
            SetUnassignedTasksErrors(context);
            SetVisibleUnassignedTasks(context);
        }

        private void SetVisibleUnassignedTasks(JiraReport context)
        {
            if (context.ReportTasks.UnassignedTasksAll.IsEmpty())
                return;

            context.ReportTasks.UnassignedTasksVisible = context.ReportTasks.UnassignedTasksAll.Where(t=> !t.IsSubtask).Take(5).ToList();
            context.ReportTasks.AdditionalUnassignedTasks = context.ReportTasks.UnassignedTasksAll.Count(t=>!t.IsSubtask) - context.ReportTasks.UnassignedTasksVisible.Count;
            context.ReportTasks.UnassignedTasksSearchUrl = new Uri(context.Settings.BaseUrl + "/issues/?jql=" + JiraApiUrls.UnassignedUncompletedIssues(context.ProjectKey, context.Sprint.Id));
        }

        void SetCompletedTasksErrors(JiraReport report)
        {
            int completedErrors = 0;
            completedErrors += GetErrorsCount(report.ReportTasks.CompletedTasksVisible);
            report.ReportTasks.CompletedTasksErrorCount = completedErrors;
        }

        static void SetUnassignedTasksErrors(JiraReport report)
        {
            SetErrors(report.ReportTasks.UnassignedTasksVisible, report.Policy);
            report.ReportTasks.UnassignedTasksErrorCount = GetErrorsCount(report.ReportTasks.UnassignedTasksVisible);
        }

        List<IssueDetailed> GetCompletedTasks(JiraReport context)
        {
            var issuesContext = GetIssuesContext(context);
            context.ReportTasks.CompletedTasksSearchUrl = new Uri(context.Settings.BaseUrl + "/issues/?jql=" + JiraApiUrls.ResolvedIssues(issuesContext.ProjectKey, TimeFormatting.DateToISO(issuesContext.StartDate), TimeFormatting.DateToISO(issuesContext.EndDate)));

            var completedTasks = new List<IssueDetailed>();
            var issues = JiraService.GetCompletedIssues(issuesContext);
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

        void SetVisibleCompletedTasks(JiraReport context)
        {
            if (context.ReportTasks.CompletedTasksAll.IsEmpty())
                return;

            context.ReportTasks.CompletedTasksVisible = context.ReportTasks.CompletedTasksAll.Take(5).ToList();
            context.ReportTasks.AdditionalCompletedTasks = context.ReportTasks.CompletedTasksAll.Count - context.ReportTasks.CompletedTasksVisible.Count;
        }

        JiraResponse<JiraIssue> GetSprintTasks(JiraReport context, string sprintId)
        {
            return JiraService.GetSprintTasks(context.JiraRequestContext, context.ProjectKey, sprintId);
        }

        void SetUnfinishedTasks(JiraResponse<JiraIssue> jiraIssues, JiraReport context)
        {
            var tasks = context.ReportTasks;
            tasks.InProgressTasks = new List<IssueDetailed>();
            tasks.OpenTasks = new List<IssueDetailed>();
            tasks.UnassignedTasksAll = new List<IssueDetailed>();
            tasks.SprintTasksAll = new List<IssueDetailed>();

            foreach (var jiraIssue in jiraIssues.Issues)
            {
                var issue = GetCompleteIssue(context, jiraIssue);
                tasks.SprintTasksAll.Add(issue);

                if (issue.StatusCategory.name == "In Progress")
                    tasks.InProgressTasks.Add(issue);
                else
                    if (issue.Resolution == null)
                    {
                        IssueAdapter.HasSubtasksInProgress(issue);
                        tasks.OpenTasks.Add(issue);
                    }
                if (issue.Assignee == null && issue.StatusCategory.name != "Done")
                    tasks.UnassignedTasksAll.Add(issue);
            }
        }

        IssueDetailed GetCompleteIssue(JiraReport context, JiraIssue jiraIssue)
        {
            var issue = new IssueDetailed(jiraIssue);
            var issueProcessor = new IssueProcessor(context) { JiraService = JiraService };

            issueProcessor.SetIssue(issue, jiraIssue);
            IssueAdapter.SetIssueErrors(issue, context.Policy);
            return issue;
        }

        void SortTasks(JiraReport context)
        {
            if (!context.IssuePriorityEnabled)
                return;

            if (context.ReportTasks.InProgressTasks != null)
                context.ReportTasks.InProgressTasks = context.ReportTasks.InProgressTasks.OrderBy(task => task.Priority.id).ToList();
            if (context.ReportTasks.OpenTasks != null)
                context.ReportTasks.OpenTasks = context.ReportTasks.OpenTasks.OrderBy(task => task.Priority.id).ToList();
            if (context.ReportTasks.UnassignedTasksAll != null)
                context.ReportTasks.UnassignedTasksAll = context.ReportTasks.UnassignedTasksAll.OrderBy(task => task.Priority.id).ToList();
        }

        List<JiraIssue> GetAditionalSprintIssues(JiraReport report, Sprint sprint)
        {
            if (sprint == null)
                return new List<JiraIssue>();

            return GetSprintTasks(report, sprint.Id.ToString()).Issues;
        }

        #region Static Helpers
        static IssueDetailed CreateParent(IssueDetailed task, JiraAuthor author)
        {
            var parent = new IssueDetailed(task.Parent);
            foreach (var subtask in parent.SubtasksDetailed)
                IssueAdapter.SetLoggedAuthor(subtask, author.Name);
            parent.SubtasksDetailed = new List<IssueDetailed>();
            return parent;
        }

        private static IssuesContext GetIssuesContext(JiraReport context)
        {
            var issuesContext = new IssuesContext
            {
                RequestContext = context.JiraRequestContext,
                ProjectKey = context.ProjectKey,
                StartDate = context.ToDate.AddDays(-6).Date,
                EndDate = context.ToDate
            };
            return issuesContext;
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
                        if (parent.SubtasksDetailed == null)
                            parent.SubtasksDetailed = new List<IssueDetailed>();
                        if (parent.SubtasksDetailed.Exists(t => t.Key == task.Key) == false)
                            parent.SubtasksDetailed.Add(new IssueDetailed(task));
                    }
                    else
                    {
                        parent = CreateParent(task, author);
                        parent.SubtasksDetailed.Add(new IssueDetailed(task));
                        IssueAdapter.SetLoggedAuthor(parent, author.Name);
                        parentTasks.Add(parent);
                    }
                }
            }
            parentTasks.RemoveAll(t => t.IsSubtask == true);
            return parentTasks;
        }

        public static void SetErrors(List<IssueDetailed> tasks, JiraPolicy policy)
        {
            if (tasks != null && tasks.Count > 0)
                foreach (var task in tasks)
                    IssueAdapter.SetIssueErrors(task, policy);
        }

        static int GetErrorsCount(List<IssueDetailed> tasks)
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

        #endregion
    }
}
