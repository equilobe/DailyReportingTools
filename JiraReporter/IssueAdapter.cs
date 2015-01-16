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
                var jiraIssue = new Issue(issue) { Entries = new List<Entries>() };
                AddEntry(jiraIssue, entry);
                AddIssue(jiraIssue, issues);
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
                var jiraIssue = new AnotherJiraRestClient.Issue();
                jiraIssue = RestApiRequests.GetIssue(issue.Key, policy);
                SetIssue(issue, policy, jiraIssue, timesheet, pullRequests);
            }
        }

        public static void SetIssue(Issue issue, SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue jiraIssue, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            SetGenericIssue(issue, policy, jiraIssue, timesheet, pullRequests);
            if (issue.SubTask == true)
            {
                SetParent(issue, jiraIssue, policy, timesheet, pullRequests);
                SetSubtasksIssues(issue.Parent, policy, timesheet, pullRequests);
            }
            if (issue.Subtasks != null)
                SetSubtasksIssues(issue, policy, timesheet, pullRequests);
        }

        public static void SetGenericIssue(Issue issue, SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue jiraIssue, Timesheet timesheet, List<PullRequest> pullRequests)
        {    
            if (issue.Entries == null)
                issue.Entries = new List<Entries>();
            issue.Priority = jiraIssue.fields.priority;
            issue.PolicyReopenedStatus = policy.AdvancedOptions.ReopenedStatus;
            if (jiraIssue.fields.assignee != null)
                issue.Assignee = jiraIssue.fields.assignee.displayName;
            issue.RemainingEstimateSeconds = jiraIssue.fields.aggregatetimeestimate;
            issue.OriginalEstimateSecondsTotal = jiraIssue.fields.aggregatetimeoriginalestimate;
            issue.OriginalEstimateSeconds = jiraIssue.fields.timeoriginalestimate;
            if (jiraIssue.fields.resolution != null)
            {
                issue.Resolution = jiraIssue.fields.resolution.name;
                issue.StringResolutionDate = jiraIssue.fields.resolutiondate;
                issue.ResolutionDate = Convert.ToDateTime(issue.StringResolutionDate);
                issue.CompletedTimeAgo = TimeFormatting.GetStringDay(issue.ResolutionDate.ToOriginalTimeZone());
            }
            issue.Status = jiraIssue.fields.status.name;
            issue.Type = jiraIssue.fields.issuetype.name;
            issue.SubTask = jiraIssue.fields.issuetype.subtask;
            SetLabel(issue, policy, jiraIssue);
            issue.StatusCategory = jiraIssue.fields.status.statusCategory;
            issue.Created = Convert.ToDateTime(jiraIssue.fields.created);
            issue.Updated = jiraIssue.fields.updated;
            issue.UpdatedDate = Convert.ToDateTime(issue.Updated);
            issue.TimeSpentTotal = jiraIssue.fields.aggregatetimespent;
            issue.TotalRemainingSeconds = jiraIssue.fields.aggregatetimeestimate;
            if (jiraIssue.fields.subtasks != null)
                issue.Subtasks = jiraIssue.fields.subtasks;
            SetIssueTimeSpent(issue);
            issue.TimeSpentOnTask = jiraIssue.fields.timespent;
            SetIssueTimeFormat(issue);
            if(timesheet!= null)
                SetIssueExists(issue, timesheet.Worklog.Issues);                
            issue.Assignee = AuthorsProcessing.GetCleanName(issue.Assignee);
            AdjustIssuePullRequests(issue, pullRequests);
            SetIssueLink(issue, policy);
            HasWorkLoggedByAssignee(issue, timesheet);
           // SetIssueErrors(issue);
            SetStatusType(issue);
            SetDisplayStatus(issue, policy);
        }

        public static void SetSubtasksIssues(Issue issue, SourceControlLogReporter.Model.Policy policy, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            var jiraIssue = new AnotherJiraRestClient.Issue();
            issue.SubtasksIssues = new List<Issue>();
            foreach (var task in issue.Subtasks)
            {
                jiraIssue = RestApiRequests.GetIssue(task.key, policy);
                issue.SubtasksIssues.Add(new Issue { Key = task.key, Summary = jiraIssue.fields.summary });
                SetGenericIssue(issue.SubtasksIssues.Last(), policy, jiraIssue, timesheet, pullRequests);
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

        private static void SetLabel(Issue issue, SourceControlLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue jiraIssue)
        {
            foreach (var label in jiraIssue.fields.labels)
                if (label == policy.AdvancedOptions.PermanentTaskLabel)
                    issue.Label = label;
        }

        public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }

        public static void SetParent(Issue issue, AnotherJiraRestClient.Issue jiraIssue, SourceControlLogReporter.Model.Policy policy, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            issue.Parent = new Issue { Key = jiraIssue.fields.parent.key, Summary = jiraIssue.fields.parent.fields.summary };
            var parent = RestApiRequests.GetIssue(issue.Parent.Key, policy);
            SetGenericIssue(issue.Parent, policy, parent, timesheet, pullRequests);
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
                        var jiraIssue = timesheet.Worklog.Issues.Find(i => i.Key == issue.Key);
                        issue.HasWorkLoggedByAssignee = jiraIssue.Entries.Exists(e => e.AuthorFullName == issue.Assignee);
                    }
              //  else issue.HasWorkLoggedByAssignee = false;
        }

        public static int GetTasksTimeLeftSeconds(List<Issue> tasks)
        {
            int sum = 0;
            foreach (var task in tasks)
                if (task.SubTask == false)
                    sum += task.TotalRemainingSeconds;
                else
                    if (tasks.Exists(t => t.Key == task.Parent.Key) == false)
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
            if(issue.SubTask == false && issue.Label != policy.AdvancedOptions.PermanentTaskLabel)
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

        public static void SetStatusType(Issue issue)
        {
            if (issue.StatusCategory.name == "In Progress")
                issue.StatusType = "In Progress";
            else
                if (issue.Resolution == null)
                    issue.StatusType = "Open";
        }

        public static void SetDisplayStatus(Issue issue, SourceControlLogReporter.Model.Policy policy)
        {
            if (policy.AdvancedOptions.WorkflowStatuses != null)
            {
                if (policy.AdvancedOptions.WorkflowStatuses.Exists(s => s == issue.Status))
                    issue.DisplayStatus = true;
            }
        }

        public static void SetLoggedAuthor(Issue issue, string authorName)
        {
            if (issue.LoggedAuthor == null)
                issue.LoggedAuthor = AuthorsProcessing.GetCleanName(authorName);
        }
        
        public static void ChangeLoggedAuthor(Issue issue, string authorName)
        {
            issue.LoggedAuthor = AuthorsProcessing.GetCleanName(authorName);
        }

        public static void SetSubtasksLoggedAuthor(Issue issue, string authorName)
        {
            if (issue.SubtasksIssues != null && issue.SubtasksIssues.Count > 0)
                foreach (var subtask in issue.SubtasksIssues)
                    SetLoggedAuthor(subtask, authorName);
        }
    }
}
