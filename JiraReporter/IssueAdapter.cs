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
        public static void RemoveWrongEntries(Issue issue, DateTime date)
        {
            if (issue.Entries != null)
            {
                var newEntries = new List<Entries>(issue.Entries);
                newEntries.RemoveAll(e => e.StartDate.ToOriginalTimeZone() < date || e.StartDate.ToOriginalTimeZone() >= date.AddDays(1));
                issue.Entries = newEntries;
            }
        }

        public static void RemoveWrongIssues(List<Issue> issues)
        {
            if (issues != null)
                issues.RemoveAll(i => i.Entries.Count == 0 && i.Commits.Count == 0);
        }

        public static void TimeSpentFromEntries(Issue issue)
        {
            issue.TimeSpent = issue.Entries.Sum(e => e.TimeSpent);
        }

        public static void SetTimeFormat(Issue issue)
        {
            if (issue.TimeSpent > 0)
                issue.TimeLogged = TimeFormatting.SetTimeFormat(issue.TimeSpent);
            else
                issue.TimeLogged = TimeFormatting.SetTimeFormat(issue.TimeSpentOnTask);
            issue.TotalRemaining = TimeFormatting.SetTimeFormat8Hour(issue.TotalRemainingSeconds);
            issue.TimeLoggedTotal = TimeFormatting.SetTimeFormat8Hour(issue.TimeSpentTotal);
            issue.RemainingEstimate = TimeFormatting.SetTimeFormat8Hour(issue.RemainingEstimateSeconds);
        }



        private static void SetIssueExists(Issue issue, List<Issue> issues)
        {
            issue.ExistsInTimesheet = IssueExistsTimesheet(issue, issues);
        }


        {
        public static void SetIssuesExistInTimesheet(List<Issue> issues, List<Issue> timesheet)
        {
            foreach (var issue in issues)
                SetIssueExists(issue, timesheet);
        }

        public static bool IssueExistsTimesheet(Issue issue, List<Issue> issues)
        {
            if (issues.Exists(i => i.Key == issue.Key))
                return true;
            return false;
        }
		public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }



        public static void AdjustIssueCommits(DayLog dayLog)
        {
            if (dayLog.Issues != null)
                foreach (var issue in dayLog.Issues)
                {
                    AdjustIssueCommits(issue, dayLog.Commits);
                    if (issue.Commits.Count > 0)
                        RemoveWrongCommits(issue, dayLog.Date);
                }
        }

        public static void AdjustIssueCommits(Issue issue, List<Commit> commits)
        {
            issue.Commits = new List<Commit>();
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

        public static void RemoveWrongCommits(Issue issue, DateTime date)
        {
            var commits = new List<Commit>(issue.Commits);
            commits.RemoveAll(c => c.Entry.Date.ToOriginalTimeZone() < date || c.Entry.Date.ToOriginalTimeZone() >= date.AddDays(1));
            issue.Commits = commits;
        }

        private static void EditIssueCommits(Issue issue)
        {
            if (issue.Commits.Count > 0)
                foreach (var commit in issue.Commits)
                    commit.TaskSynced = true;
        }

        public static int GetTasksTimeLeftSeconds(List<Issue> tasks)
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

        public static void HasTasksInProgress(Issue task)
        {
            if (task.Subtasks.Count > 0)
            {
                task.HasSubtasksInProgress = HasSubtasksInProgress(task);
                task.HasAssignedSubtasksInProgress = HasAssignedSubtasksInProgress(task);
            }
        }

        public static bool HasSubtasksInProgress(Issue task)
        {
            if (task.Resolution == null && task.StatusCategory.name != "In Progess" && task.SubtasksIssues.Exists(s => s.StatusCategory.name == "In Progress"))
                return true;
            return false;
        }

        public static bool HasAssignedSubtasksInProgress(Issue task)
        {
            if (HasSubtasksInProgress(task) == true && task.SubtasksIssues.Exists(s => s.Assignee == task.Assignee))
                return true;
            return false;
        }

        public static void SetIssueErrors(Issue issue, SourceControlLogReporter.Model.Policy policy)
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



        public static void SetLoggedAuthor(Issue issue, string authorName)
        {
                issue.LoggedAuthor = AuthorHelpers.GetCleanName(authorName);
        }

        public static void SetSubtasksLoggedAuthor(Issue issue, string authorName)
        {
            if (issue.SubtasksIssues != null && issue.SubtasksIssues.Count > 0)
                foreach (var subtask in issue.SubtasksIssues)
                    SetLoggedAuthor(subtask, authorName);
        }
    }
}
