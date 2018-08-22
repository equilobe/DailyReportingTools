using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Policy;
using JiraReporter.Helpers;
using Equilobe.DailyReport.Utils;

namespace JiraReporter
{
    class IssueAdapter
    {
        public static void RemoveWrongEntries(EntryContext context)
        {
            if (context.Issue.Entries == null)
                return;

            context.Issue.Entries
                .RemoveWhere(e => e.StartedAt.ToOriginalTimeZone(context.OffsetFromUtc) < context.FromDate)
                .RemoveWhere(e => e.StartedAt.ToOriginalTimeZone(context.OffsetFromUtc) >= context.ToDate)
                .RemoveWhere(e => context.AuthorName != e.AuthorFullName);
        }

        public static void RemoveWrongIssues(List<IssueDetailed> issues)
        {
            if (issues != null)
                issues.RemoveAll(i => i.Entries.Count == 0 && i.Commits.Count == 0);
        }

        public static void TimeSpentFromEntries(IssueDetailed issue)
        {
            issue.TimeSpent = issue.Entries.Sum(e => e.TimeSpent);
        }

        public static void SetTimeFormat(IssueDetailed issue)
        {
            if (issue.TimeSpent > 0)
                issue.TimeLogged = issue.TimeSpent.SetTimeFormat();
            else
                issue.TimeLogged = issue.TimeSpentOnTask.SetTimeFormat();
            issue.TotalRemaining = issue.TotalRemainingSeconds.SetTimeFormat8Hour();
            issue.TimeLoggedTotal = issue.TimeSpentTotal.SetTimeFormat8Hour();
            issue.RemainingEstimate = issue.RemainingEstimateSeconds.SetTimeFormat8Hour();
        }

        private static void SetIssueExists(IssueDetailed issue, List<IssueDetailed> issues)
        {
            issue.ExistsInTimesheet = IssueExistsTimesheet(issue, issues);
        }

        public static void SetIssuesExistInTimesheet(List<IssueDetailed> issues, List<IssueDetailed> timesheet)
        {
            foreach (var issue in issues)
                SetIssueExists(issue, timesheet);
        }

        public static bool IssueExistsTimesheet(IssueDetailed issue, List<IssueDetailed> issues)
        {
            if (issues.Exists(i => i.Key == issue.Key))
                return true;
            return false;
        }
        public static List<IssueDetailed> OrderIssues(List<IssueDetailed> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }

        public static void AdjustIssueCommits(JiraDayLog dayLog, TimeSpan offsetFromUtc)
        {
            if (dayLog.Issues != null)
                foreach (var issue in dayLog.Issues)
                {
                    AdjustIssueCommits(issue, dayLog.Commits);
                    if (issue.Commits.Count > 0)
                        RemoveWrongCommits(issue, dayLog.Date, offsetFromUtc);
                }
        }

        public static void AdjustIssueCommits(IssueDetailed issue, List<JiraCommit> commits)
        {
            issue.Commits = new List<JiraCommit>();
            issue.Commits = commits.FindAll(commit => ContainsKey(commit.Entry.Message, issue.Key) == true);
            EditIssueCommits(issue);
        }

        public static bool ContainsKey(string message, string key)
        {
            string msg = message.ToLower();
            string keyLower = key.ToLower();
            keyLower = Regex.Replace(keyLower, "[^A-Za-z0-9 ]", "");
            msg = Regex.Replace(msg, "[^A-Za-z0-9 ]", "");
            keyLower = keyLower.Replace(" ", "");
            msg = msg.Replace(" ", "");
            return msg.Contains(keyLower);
        }

        public static void RemoveWrongCommits(IssueDetailed issue, DateTime date, TimeSpan offsetFromUtc)
        {
            var commits = new List<JiraCommit>(issue.Commits);
            commits.RemoveAll(c => c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) < date || c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) >= date.AddDays(1));
            issue.Commits = commits;
        }

        private static void EditIssueCommits(IssueDetailed issue)
        {
            if (issue.Commits.Count > 0)
                foreach (var commit in issue.Commits)
                    commit.TaskSynced = true;
        }

        public static int GetTasksTimeLeftSeconds(List<IssueDetailed> tasks)
        {
            int sum = 0;
            foreach (var task in tasks)
                if (!task.IsSubtask)
                    sum += task.TotalRemainingSeconds;
                else
                    if (!tasks.Exists(t => t.Key == task.Parent.Key))
                        sum += task.RemainingEstimateSeconds;
            return sum;
        }

        public static void HasTasksInProgress(IssueDetailed task)
        {
            if (task.Subtasks.Count > 0)
            {
                task.HasSubtasksInProgress = HasSubtasksInProgress(task);
                task.HasAssignedSubtasksInProgress = HasAssignedSubtasksInProgress(task);
            }
        }

        public static bool HasSubtasksInProgress(IssueDetailed task)
        {
            if (task.Resolution == null && task.StatusCategory.name != "In Progess" && task.SubtasksDetailed.Exists(s => s.StatusCategory.name == "In Progress"))
                return true;
            return false;
        }

        public static bool HasAssignedSubtasksInProgress(IssueDetailed task)
        {
            if (HasSubtasksInProgress(task) == true && task.SubtasksDetailed.Exists(s => s.Assignee == task.Assignee))
                return true;
            return false;
        }

        public static void SetIssueErrors(IssueDetailed issue, JiraPolicy policy)
        {
            issue.Errors = new List<Error>();
            if (issue.IsSubtask == false && issue.Label != policy.AdvancedOptions.PermanentTaskLabel)
            {
                if (issue.StatusCategory.name == "Done")
                {
                    if (issue.TimeSpentTotal == 0)
                    {
                        issue.Errors.Add(new Error { Type = ErrorType.HasNoTimeSpent });
                        issue.ErrorsCount++;
                    }
                    if (issue.RemainingEstimateSeconds > 0)
                    {
                        issue.Errors.Add(new Error { Type = ErrorType.HasRemaining });
                        issue.ErrorsCount++;
                    }
                }
                else
                {
                    if (issue.RemainingEstimateSeconds == 0)
                    {
                        issue.Errors.Add(new Error { Type = ErrorType.HasNoRemaining });
                        issue.ErrorsCount++;
                    }
                }
            }
        }



        public static void SetLoggedAuthor(IssueDetailed issue, string authorName)
        {
            issue.LoggedAuthor = AuthorHelpers.GetCleanName(authorName);
        }

        public static void SetSubtasksLoggedAuthor(IssueDetailed issue, string authorName)
        {
            if (issue.SubtasksDetailed != null && issue.SubtasksDetailed.Count > 0)
                foreach (var subtask in issue.SubtasksDetailed)
                    SetLoggedAuthor(subtask, authorName);
        }

        public static IssueDetailed GetBasicIssue(JiraIssue issue)
        {
            var basicIssue = new IssueDetailed
            {
                Key = issue.key,
                Summary = issue.fields.summary
            };
            GetEntriesFromJiraWorklogs(issue, basicIssue);

            return basicIssue;
        }

        public static void GetEntriesFromJiraWorklogs(JiraIssue issue, IssueDetailed basicIssue)
        {
            if (issue.fields.worklog != null && issue.fields.worklog.worklogs != null)
                basicIssue.Entries = issue.fields.worklog.worklogs.Select(wk => new Entry
                {
                    Author = wk.author.name,
                    AuthorFullName = wk.author.displayName,
                    StartedAt = Convert.ToDateTime(wk.started),
                    CreatedAt = Convert.ToDateTime(wk.created),
                    UpdatedAt = Convert.ToDateTime(wk.updated),
                    Comment = wk.comment,
                    TimeSpent = wk.timeSpentSeconds,
                    UpdateAuthor = wk.updateAuthor.name,
                    UpdateAuthorFullName = wk.updateAuthor.displayName
                }).ToList();
        }
    }
}
