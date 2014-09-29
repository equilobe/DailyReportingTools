using JiraReporter.Model;
using System;
using System.Collections.Generic;
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

        public static void RemoveEntries(List<Issue> issues)
        {
            foreach (var issue in issues)
                issue.Entries.RemoveAll(e => e.AuthorFullName != issue.Entries.First().AuthorFullName);
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

        private static void AddIssue(Issue issue, List<Issue> issues)
        {
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
            {
                issue.Subtasks = newIssue.fields.subtasks;
            }

            if (issue.Entries != null)
                SetIssueTimeSpent(issue);
            else
                issue.TimeSpent = newIssue.fields.timespent;
            SetIssueTimeFormat(issue);
            SetIssueExists(issue, timesheet.Worklog.Issues);
            if (issue.SubTask == true)
                SetParent(issue, newIssue, policy);

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

        private static void SetIssueTimeSpent(Issue issue)
        {
            foreach (var entry in issue.Entries)
                issue.TimeSpent += entry.TimeSpent;
        }

        public static void SetIssueTimeFormat(Issue issue)
        {
            issue.TimeLogged = TimeFormatting.SetTimeFormat8Hour(issue.TimeSpent);
            if (issue.Subtasks != null)
            {
                issue.TotalRemaining = TimeFormatting.SetTimeFormat8Hour(issue.TotalRemainingSeconds);
                if (issue.Subtasks.Count > 0)
                    issue.TimeLoggedTotal = TimeFormatting.SetTimeFormat8Hour(issue.TimeSpentTotal);
            }
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

        private static void SetParent(Issue issue, AnotherJiraRestClient.Issue newIssue, SvnLogReporter.Model.Policy policy)
        {
            issue.Parent = new Issue { Key = newIssue.fields.parent.key, Summary = newIssue.fields.parent.fields.summary };
            SetIssueLink(issue.Parent, policy);
        }

        public static void AdjustIssueCommits(Author author)
        {
            var find = new List<Commit>();
            if (author.Issues != null)
                foreach (var issue in author.Issues)
                {
                    issue.Commits = new List<Commit>();
                    find = author.Commits.FindAll(commit => commit.Entry.Message.Contains(issue.Key) == true);
                    if (find != null)                    
                        issue.Commits = find;                  
                    EditIssueCommits(issue);
                }
        }

        private static void EditIssueCommits(Issue issue)
        {
            if (issue.Commits.Count > 0)
                foreach (var commit in issue.Commits)
                    commit.TaskSynced = true;
        }
    }
}
