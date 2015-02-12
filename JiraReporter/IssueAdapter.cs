using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Report;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JiraReporter
{
    class IssueAdapter
    {
        public static void RemoveWrongEntries(CompleteIssue issue, DateTime date, TimeSpan offsetFromUtc)
        {
            if (issue.Entries != null)
            {
                var newEntries = new List<Entries>(issue.Entries);
                newEntries.RemoveAll(e => e.StartDate.ToOriginalTimeZone(offsetFromUtc) < date || e.StartDate.ToOriginalTimeZone(offsetFromUtc) >= date.AddDays(1));
                issue.Entries = newEntries;
            }
        }

        public static void RemoveWrongIssues(List<CompleteIssue> issues)
        {
            if (issues != null)
                issues.RemoveAll(i => i.Entries.Count == 0 && i.Commits.Count == 0);
        }

        public static void TimeSpentFromEntries(CompleteIssue issue)
        {
            issue.TimeSpent = issue.Entries.Sum(e => e.TimeSpent);
        }

        public static void SetTimeFormat(CompleteIssue issue)
        {
            if (issue.TimeSpent > 0)
                issue.TimeLogged = issue.TimeSpent.SetTimeFormat();
            else
                issue.TimeLogged = issue.TimeSpentOnTask.SetTimeFormat();
            issue.TotalRemaining = issue.TotalRemainingSeconds.SetTimeFormat8Hour();
            issue.TimeLoggedTotal = issue.TimeSpentTotal.SetTimeFormat8Hour();
            issue.RemainingEstimate = issue.RemainingEstimateSeconds.SetTimeFormat8Hour();
        }



        private static void SetIssueExists(CompleteIssue issue, List<CompleteIssue> issues)
        {
            issue.ExistsInTimesheet = IssueExistsTimesheet(issue, issues);
        }

        public static void SetIssuesExistInTimesheet(List<CompleteIssue> issues, List<CompleteIssue> timesheet)
        {
            foreach (var issue in issues)
                SetIssueExists(issue, timesheet);
        }

        public static bool IssueExistsTimesheet(CompleteIssue issue, List<CompleteIssue> issues)
        {
            if (issues.Exists(i => i.Key == issue.Key))
                return true;
            return false;
        }
		public static List<CompleteIssue> OrderIssues(List<CompleteIssue> issues)
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

        public static void AdjustIssueCommits(CompleteIssue issue, List<JiraCommit> commits)
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

        public static void RemoveWrongCommits(CompleteIssue issue, DateTime date, TimeSpan offsetFromUtc)
        {
            var commits = new List<JiraCommit>(issue.Commits);
            commits.RemoveAll(c => c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) < date || c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) >= date.AddDays(1));
            issue.Commits = commits;
        }

        private static void EditIssueCommits(CompleteIssue issue)
        {
            if (issue.Commits.Count > 0)
                foreach (var commit in issue.Commits)
                    commit.TaskSynced = true;
        }

        public static int GetTasksTimeLeftSeconds(List<CompleteIssue> tasks)
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

        public static void HasTasksInProgress(CompleteIssue task)
        {
            if (task.Subtasks.Count > 0)
            {
                task.HasSubtasksInProgress = HasSubtasksInProgress(task);
                task.HasAssignedSubtasksInProgress = HasAssignedSubtasksInProgress(task);
            }
        }

        public static bool HasSubtasksInProgress(CompleteIssue task)
        {
            if (task.Resolution == null && task.StatusCategory.name != "In Progess" && task.SubtasksIssues.Exists(s => s.StatusCategory.name == "In Progress"))
                return true;
            return false;
        }

        public static bool HasAssignedSubtasksInProgress(CompleteIssue task)
        {
            if (HasSubtasksInProgress(task) == true && task.SubtasksIssues.Exists(s => s.Assignee == task.Assignee))
                return true;
            return false;
        }

        public static void SetIssueErrors(CompleteIssue issue, JiraPolicy policy)
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
                        ErrorType type = new ErrorType();
                        if (issue.Assignee == null)
                            type = ErrorType.Unassigned;
                        else
                            type = ErrorType.HasNoRemaining;
                        issue.Errors.Add(new Error { Type = type });
                        issue.ErrorsCount++;
                    }
                }
            }
        }



        public static void SetLoggedAuthor(CompleteIssue issue, string authorName)
        {
                issue.LoggedAuthor = AuthorHelpers.GetCleanName(authorName);
        }

        public static void SetSubtasksLoggedAuthor(CompleteIssue issue, string authorName)
        {
            if (issue.SubtasksIssues != null && issue.SubtasksIssues.Count > 0)
                foreach (var subtask in issue.SubtasksIssues)
                    SetLoggedAuthor(subtask, authorName);
        }
    }
}
