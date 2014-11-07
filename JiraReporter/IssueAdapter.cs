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
        public static void SetIssueEntries(List<Entries> entries, Issue issue, List<Issue> issues)
        {
            foreach (var entry in entries)
                if (entry.AuthorFullName != entries.First().AuthorFullName)
                    AddEntries(issues, entry, issue);
        }

        public static void RemoveWrongEntries(List<Issue> issues)
        {
            foreach (var issue in issues)
                issue.Entries.RemoveAll(e => e.AuthorFullName != issue.Entries.First().AuthorFullName);
        }

        public static void RemoveWrongEntries(Issue issue, DateTime date)
        {
            if(issue.Entries!=null)
            {
                var newEntries = new List<Entries>(issue.Entries);
                newEntries.RemoveAll(e => e.StartDate.ToOriginalTimeZone() < date || e.StartDate.ToOriginalTimeZone() >= date.AddDays(1));
                issue.Entries = newEntries;
            }            
        }

        public static void RemoveWrongIssues(List<Issue> issues)
        {
            if(issues!=null)
                issues.RemoveAll(i => i.Entries.Count == 0 && i.Commits.Count == 0);   
        }

        private static void AddEntries(List<Issue> issues, Entries entry, Issue issue)
        {
            var existsIssue = IssueExists(entry, issues, issue);
            if (existsIssue != null)
                AddEntry(existsIssue, entry);
            else
            {
                var newIssue = new Issue(issue) { Entries = new List<Entries>() };
                AddEntry(newIssue, entry);
                AddIssue(newIssue, issues);
            }
        }

        private static Issue IssueExists(Entries entry, List<Issue> issues, Issue issue)
        {
            return issues.Find(i => i.Entries.First().AuthorFullName == entry.AuthorFullName && i.Key == issue.Key);
        }

        private static bool IssueExistsTimesheet(Issue issue, List<Issue> issues)
        {
            if (issues.Exists(i => i.Key == issue.Key))
                return true;
            return false;
        }

        public static void AddIssue(Issue issue, List<Issue> issues)
        {
            if (issues == null)
                issues = new List<Issue>();
            issues.Add(issue);
        }

        private static void AddEntry(Issue issue, Entries entry)
        {
            issue.Entries.Add(entry);
        }

        public static void SetIssues(Timesheet timesheet, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            foreach (var issue in timesheet.Worklog.Issues)
            {
                var newIssue = new AnotherJiraRestClient.Issue();
                newIssue = RestApiRequests.GetIssue(issue.Key, policy);
                SetIssue(issue, policy, newIssue, timesheet, pullRequests);
                if (issue.SubTask == true)
                    SetParent(issue, newIssue, policy, timesheet, pullRequests);
                if (issue.Subtasks != null)
                    SetSubtasksIssues(issue, policy, timesheet, pullRequests);
            }
        }

        public static void SetIssue(Issue issue, SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue newIssue, Timesheet timesheet, List<PullRequest> pullRequests)
        {    
                if (issue.Entries == null)
                    issue.Entries = new List<Entries>();
                issue.Priority = newIssue.fields.priority;
                issue.PolicyReopenedStatus = policy.ReopenedStatus;
                if (newIssue.fields.assignee != null)
                    issue.Assignee = newIssue.fields.assignee.displayName;
                issue.RemainingEstimateSeconds = newIssue.fields.timeestimate;
                issue.OriginalEstimateSecondsTotal = newIssue.fields.aggregatetimeoriginalestimate;
                issue.OriginalEstimateSeconds = newIssue.fields.timeoriginalestimate;
                if (newIssue.fields.resolution != null)
                {
                    issue.Resolution = newIssue.fields.resolution.name;
                    issue.StringResolutionDate = newIssue.fields.resolutiondate;
                    issue.ResolutionDate = Convert.ToDateTime(issue.StringResolutionDate);
                    issue.CompletedTimeAgo = TimeFormatting.GetStringDay(issue.ResolutionDate.ToOriginalTimeZone());
                }
                issue.Status = newIssue.fields.status.name;
                issue.Type = newIssue.fields.issuetype.name;
                issue.SubTask = newIssue.fields.issuetype.subtask;
                SetLabel(issue, policy, newIssue);
                issue.StatusCategory = newIssue.fields.status.statusCategory;
                issue.Created = Convert.ToDateTime(newIssue.fields.created);
                issue.Updated = newIssue.fields.updated;
                issue.UpdatedDate = Convert.ToDateTime(issue.Updated);
                issue.TimeSpentTotal = newIssue.fields.aggregatetimespent;
                issue.TotalRemainingSeconds = newIssue.fields.aggregatetimeestimate;
                if (newIssue.fields.subtasks != null)
                    issue.Subtasks = newIssue.fields.subtasks;
                SetIssueTimeSpent(issue);
                issue.TimeSpentOnTask = newIssue.fields.timespent;
                SetIssueTimeFormat(issue);
                SetIssueExists(issue, timesheet.Worklog.Issues);                
                issue.Assignee = AuthorsProcessing.SetName(issue.Assignee);
                AdjustIssuePullRequests(issue, pullRequests);
                SetIssueLink(issue, policy);
                HasWorkLoggedByAssignee(issue, timesheet);
        }

        public static void SetSubtasksIssues(Issue issue, SourceControlLogReporter.Model.Policy policy, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            var newIssue = new AnotherJiraRestClient.Issue();
            issue.SubtasksIssues = new List<Issue>();
            foreach (var task in issue.Subtasks)
            {
                newIssue = RestApiRequests.GetIssue(task.key, policy);
                issue.SubtasksIssues.Add(new Issue { Key = task.key, Summary = newIssue.fields.summary });
                SetIssue(issue.SubtasksIssues.Last(), policy, newIssue, timesheet, pullRequests);
            }
        }

        public static void SetIssueTimeSpent(Issue issue)
        {
                issue.TimeSpent = issue.Entries.Sum(e => e.TimeSpent);             
        }

        public static void SetIssueTimeFormat(Issue issue)
        {
            if (issue.TimeSpent > 0)
                issue.TimeLogged = TimeFormatting.SetTimeFormat(issue.TimeSpent);
            else
                issue.TimeLogged = TimeFormatting.SetTimeFormat(issue.TimeSpentOnTask);
            issue.TotalRemaining = TimeFormatting.SetTimeFormat8Hour(issue.TotalRemainingSeconds);
            issue.TimeLoggedTotal = TimeFormatting.SetTimeFormat8Hour(issue.TimeSpentTotal);
            issue.RemainingEstimate = TimeFormatting.SetTimeFormat8Hour(issue.RemainingEstimateSeconds);
        }

        private static void SetIssueLink(Issue issue, SourceControlLogReporter.Model.Policy policy)
        {
            Uri baseLink = new Uri(policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            issue.Link = new Uri(baseLink, issue.Key);
        }

        private static void SetIssueExists(Issue issue, List<Issue> issues)
        {
            issue.ExistsInTimesheet = IssueExistsTimesheet(issue, issues);
        }

        private static void SetLabel(Issue issue, SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue newIssue)
        {
            foreach (var label in newIssue.fields.labels)
                if (label == policy.PermanentTaskLabel)
                    issue.Label = label;
        }

        public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }

        public static void SetParent(Issue issue, AnotherJiraRestClient.Issue newIssue, SourceControlLogReporter.Model.Policy policy, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            issue.Parent = new Issue { Key = newIssue.fields.parent.key, Summary = newIssue.fields.parent.fields.summary };
            var parent = RestApiRequests.GetIssue(issue.Parent.Key, policy);
            SetIssue(issue.Parent, policy, parent, timesheet, pullRequests);
        }

        public static void AdjustIssueCommits(DayLog dayLog)
        {
            if(dayLog.Issues!=null)
                foreach(var issue in dayLog.Issues)
                {
                    AdjustIssueCommits(issue, dayLog.Commits);
                    if(issue.Commits.Count>0)
                        RemoveWrongCommits(issue, dayLog.Date);
                }
        }

        public static void AdjustIssueCommits(Issue issue, List<Commit> commits)
        {
            issue.Commits = new List<Commit>();
            issue.Commits = commits.FindAll(commit => ContainsKey(commit.Entry.Message, issue.Key) == true);
            EditIssueCommits(issue);
        }

        private static bool ContainsKey(string message, string key)
        {
            string msg = message.ToLower();
            string keyLower = key.ToLower();
            keyLower = Regex.Replace(keyLower, "[^A-Za-z0-9 ]", "");
            msg = Regex.Replace(msg, "[^A-Za-z0-9 ]", "");
            keyLower = keyLower.Replace(" ","");
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

        public static void AdjustIssuePullRequests(Issue issue, List<PullRequest> pullRequests)
        {
            if (pullRequests != null)
            {
                issue.PullRequests = new List<PullRequest>();
                issue.PullRequests = pullRequests.FindAll(pr => ContainsKey(pr.GithubPullRequest.Title, issue.Key) == true);
                EditIssuePullRequests(issue);
            }
        }

       private static void EditIssuePullRequests(Issue issue)
        {
            if (issue.PullRequests != null)
                if (issue.PullRequests.Count > 0)
                    foreach (var pullRequest in issue.PullRequests)
                        pullRequest.TaskSynced = true;
        }

        private static void HasWorkLoggedByAssignee(Issue issue, Timesheet timesheet)
        {
            if (issue != null)
                if (issue.Assignee != null)
                    if(issue.ExistsInTimesheet == true)
                    {
                        var newIssue = timesheet.Worklog.Issues.Find(i => i.Key == issue.Key);
                        issue.HasWorkLoggedByAssignee = newIssue.Entries.Exists(e => e.AuthorFullName == issue.Assignee);
                    }
              //  else issue.HasWorkLoggedByAssignee = false;
        }

        public static int GetTasksTimeLeftSeconds(List<Issue> tasks)
        {
            return tasks.Sum(t => t.TotalRemainingSeconds);
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

    }
}
