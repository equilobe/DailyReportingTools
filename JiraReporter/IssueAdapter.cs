using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class IssueAdapter
    {
        public static void SetIssueEntries(List<Entries> entries, Issue issue, List<Issue> issues)
        {
            foreach (var entry in entries)
                if (entry.AuthorFullName != entries.First().AuthorFullName)
                {
                    AddEntries(issues, entry, issue);
                }
        }

        public static void RemoveWrongEntries(List<Issue> issues)
        {
            foreach (var issue in issues)
                issue.Entries.RemoveAll(e => e.AuthorFullName != issue.Entries.First().AuthorFullName);
        }

        public static void RemoveWrongEntries(Issue issue, DateTime date)
        {
            var newEntries = new List<Entries>(issue.Entries);
            newEntries.RemoveAll(e => e.StartDate.Date != date.Date);
            issue.Entries = newEntries;
        }

        public static void RemoveWrongIssues(List<Issue> issues)
        {
            if(issues!=null)
                issues.RemoveAll(i => i.Entries.Count == 0 && i.PullRequests.Count == 0 && i.Commits.Count == 0);                    
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

        public static void SetIssues(Timesheet timesheet, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options)
        {
            foreach (var issue in timesheet.Worklog.Issues)
            {
                var newIssue = new AnotherJiraRestClient.Issue();
                newIssue = RestApiRequests.GetIssue(issue.Key, policy);
                SetIssue(issue, policy, newIssue, timesheet);
                if (issue.Subtasks != null)
                    SetSubtasksIssues(issue, policy, timesheet);
            }
        }

        public static void SetIssue(Issue issue, SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue newIssue, Timesheet timesheet)
        {

            issue.Priority = newIssue.fields.priority;
            issue.ReopenedStatus = policy.ReopenedStatus;
            if (newIssue.fields.assignee != null)
                issue.Assignee = newIssue.fields.assignee.displayName;
            if (newIssue.fields.timetracking != null)
                issue.RemainingEstimateSeconds = newIssue.fields.timetracking.remainingEstimateSeconds;
            issue.OriginalEstimateSecondsTotal = newIssue.fields.aggregatetimeoriginalestimate;
            issue.OriginalEstimateSeconds = newIssue.fields.timeoriginalestimate;
            if (newIssue.fields.resolution != null)
            {
                issue.Resolution = newIssue.fields.resolution.name;
                issue.ResolutionDate = newIssue.fields.resolutiondate;
            }
            issue.Status = newIssue.fields.status.name;
            issue.Type = newIssue.fields.issuetype.name;
            issue.SubTask = newIssue.fields.issuetype.subtask;
            SetLabel(issue, policy, newIssue);
            issue.StatusCategory = newIssue.fields.status.statusCategory;
            issue.Created = Convert.ToDateTime(newIssue.fields.created);
            issue.Updated = newIssue.fields.updated;
            issue.TimeSpentTotal = newIssue.fields.aggregatetimespent;
            issue.TotalRemainingSeconds = newIssue.fields.aggregatetimeestimate;
            if (newIssue.fields.subtasks != null)
                issue.Subtasks = newIssue.fields.subtasks;

            if (issue.Entries != null)
                SetIssueTimeSpent(issue);
            
            issue.TimeSpentOnTask = newIssue.fields.timespent;
            SetIssueTimeFormat(issue);
            SetIssueExists(issue, timesheet.Worklog.Issues);
            if (issue.SubTask == true)
                SetParent(issue, newIssue, policy, timesheet);
            issue.Assignee = AuthorsProcessing.SetName(issue.Assignee);
            
            SetIssueLink(issue, policy);
        }

        public static void SetSubtasksIssues(Issue issue, SvnLogReporter.Model.Policy policy, Timesheet timesheet)
        {
            var newIssue = new AnotherJiraRestClient.Issue();
            issue.SubtasksIssues = new List<Issue>();
            foreach (var task in issue.Subtasks)
            {
                newIssue = RestApiRequests.GetIssue(task.key, policy);
                issue.SubtasksIssues.Add(new Issue { Key = task.key, Summary = newIssue.fields.summary });
                SetIssue(issue.SubtasksIssues.Last(), policy, newIssue, timesheet);
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

        private static void SetIssueLink(Issue issue, SvnLogReporter.Model.Policy policy)
        {
            Uri baseLink = new Uri(policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            issue.Link = new Uri(baseLink, issue.Key);
        }

        private static void SetIssueExists(Issue issue, List<Issue> issues)
        {
            if (IssueExistsTimesheet(issue, issues) == true)
                issue.ExistsInTimesheet = true;
            else
                issue.ExistsInTimesheet = false;
        }

        private static void SetLabel(Issue issue, SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue newIssue)
        {
            foreach (var label in newIssue.fields.labels)
                if (label == policy.PermanentTaskLabel)
                    issue.Label = label;
        }

        public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }

        private static void SetParent(Issue issue, AnotherJiraRestClient.Issue newIssue, SvnLogReporter.Model.Policy policy, Timesheet timesheet)
        {
            issue.Parent = new Issue { Key = newIssue.fields.parent.key, Summary = newIssue.fields.parent.fields.summary };
            var parent = RestApiRequests.GetIssue(issue.Parent.Key, policy);
            SetIssue(issue.Parent, policy, parent, timesheet);
        }

        //public static void AdjustIssueCommits(Author author)
        //{
        //    if (author.Issues != null)
        //        foreach (var issue in author.Issues)
        //        {
        //            AdjustIssueCommits(issue, author.Commits);
        //        }
        //}

        public static void AdjustIssueCommits(DayLog dayLog)
        {
            if(dayLog.Issues!=null)
                foreach(var issue in dayLog.Issues)
                {
                    AdjustIssueCommits(issue, dayLog.Commits);
                    RemoveWrongCommits(issue, dayLog.Date.Date);
                }
        }

        public static void AdjustIssueCommits(Issue issue, List<Commit> commits)
        {
            //var find = new List<Commit>();
            issue.Commits = new List<Commit>();
            issue.Commits = commits.FindAll(commit => commit.Entry.Message.Contains(issue.Key) == true);
            //if (find.Count > 0)
            //    issue.Commits = find;
            EditIssueCommits(issue);
        }

        public static void RemoveWrongCommits(Issue issue, DateTime date)
        {
            var commits = new List<Commit>(issue.Commits);
            if (commits != null)
                commits.RemoveAll(c => c.Entry.Date.Date != date.Date);
            issue.Commits = commits;
        }

        private static void EditIssueCommits(Issue issue)
        {
            if (issue.Commits.Count > 0)
                foreach (var commit in issue.Commits)
                    commit.TaskSynced = true;
        }

        public static void AdjustIssuePullRequests(Author author)
        {
            if (author.PullRequests != null)
            {
                if (author.Issues != null)
                    foreach (var issue in author.Issues)
                        AdjustIssuePullRequests(issue, author.PullRequests);
            }
        }

        public static void AdjustIssuePullRequests(Issue issue, List<PullRequest> pullRequests)
        {
            //var find = new List<PullRequest>();
            issue.PullRequests = new List<PullRequest>();
            issue.PullRequests = pullRequests.FindAll(pr => pr.GithubPullRequest.Title.Contains(issue.Key) == true);
            //if (find != null)
            //    issue.PullRequests = find;
            EditIssuePullRequests(issue);
        }

       private static void EditIssuePullRequests(Issue issue)
        {
            if (issue.PullRequests != null)
                if (issue.PullRequests.Count > 0)
                    foreach (var pullRequest in issue.PullRequests)
                        pullRequest.TaskSynced = true;
        }

    }
}
