using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.JiraOriginals;
using Equilobe.DailyReport.Models.ReportPolicy;
using Equilobe.DailyReport.SL;
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
            var unfinishedTasks = GetUnfinishedTasks(context);
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

        public List<CompleteIssue> GetCompletedTasks(JiraReport context)
        {
            var completedTasks = new List<CompleteIssue>();
            var issues = new JiraService().GetCompletedIssues(context.Settings, context.ReportDate.AddDays(-6), context.ReportDate);
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
            return new JiraService().GetSprintTasks(context.Settings, context.Policy.GeneratedProperties.ProjectKey);
        }

        public void SetUnfinishedTasks(JiraIssues jiraIssues, JiraReport context)
        {
            var tasks = context.SprintTasks;
            tasks.InProgressTasks = new List<CompleteIssue>();
            tasks.OpenTasks = new List<CompleteIssue>();
            tasks.UnassignedTasks = new List<CompleteIssue>();

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

        private static CompleteIssue GetCompleteIssue(JiraReport context, JiraIssue jiraIssue)
        {
            var issue = new CompleteIssue(jiraIssue);
            var issueProcessor = new IssueProcessor(context);

            issueProcessor.SetIssue(issue, jiraIssue);
            IssueAdapter.SetIssueErrors(issue, context.Policy);
            return issue;
        }

        public IEnumerable<IGrouping<string, CompleteIssue>> GroupCompletedTasks(List<CompleteIssue> completedTasks, JiraReport context)
        {
            var tasks = from task in completedTasks
                        group task by task.CompletedTimeAgo into newGroup
                        orderby newGroup.Min(g => g.ResolutionDate.ToOriginalTimeZone(context.OffsetFromUtc))
                        select newGroup;
            tasks = tasks.OrderByDescending(t => t.Min(g => g.ResolutionDate));
            return tasks;
        }

        public void SetCompletedTasks(IEnumerable<IGrouping<string, CompleteIssue>> tasks, SprintTasks sprintTasks)
        {
            var completedTasks = new Dictionary<string, List<CompleteIssue>>();
            var issues = new List<CompleteIssue>();
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

        public static List<CompleteIssue> GetParentTasks(List<CompleteIssue> tasks, JiraAuthor author)
        {
            List<CompleteIssue> parentTasks = new List<CompleteIssue>(tasks);
            foreach (var task in tasks)
            {
                IssueAdapter.SetSubtasksLoggedAuthor(task, author.Name);
                if (task.IsSubtask == true)
                {
                    IssueAdapter.SetLoggedAuthor(task, author.Name);
                    var parentIssue = parentTasks.Find(t => t.Key == task.Parent.Key);
                    var parent = new CompleteIssue();
                    if (parentIssue != null)
                    {
                        parent = new CompleteIssue(parentIssue);
                        if (parent.AssigneeSubtasks == null)
                            parent.AssigneeSubtasks = new List<CompleteIssue>();
                        if (parent.AssigneeSubtasks.Exists(t => t.Key == task.Key) == false)
                            parent.AssigneeSubtasks.Add(new CompleteIssue(task));
                    }
                    else
                    {
                        parent = CreateParent(task, author);
                        parent.AssigneeSubtasks.Add(new CompleteIssue(task));
                        IssueAdapter.SetLoggedAuthor(parent, author.Name);
                        parentTasks.Add(parent);
                    }
                }
            }
            parentTasks.RemoveAll(t => t.IsSubtask == true);
            return parentTasks;
        }

        private static CompleteIssue CreateParent(CompleteIssue task, JiraAuthor author)
        {
            var parent = new CompleteIssue(task.Parent);
            foreach (var subtask in parent.SubtasksIssues)
                IssueAdapter.SetLoggedAuthor(subtask, author.Name);
            parent.AssigneeSubtasks = new List<CompleteIssue>();
            return parent;
        }

        public static void SetErrors(List<CompleteIssue> tasks, JiraPolicy policy)
        {
            if (tasks != null && tasks.Count > 0)
                foreach (var task in tasks)
                    IssueAdapter.SetIssueErrors(task, policy);
        }

        public static int GetErrorsCount(List<CompleteIssue> tasks)
        {
            if (tasks != null)
                return tasks.Sum(t => t.ErrorsCount);
            else
                return 0;
        }

        public static int GetTimeLeftForSpecificAuthorTasks(List<CompleteIssue> tasks, string authorName)
        {
            return tasks.Where(t => t.Assignee == authorName).Sum(i => i.TotalRemainingSeconds);
        }
    }
}
