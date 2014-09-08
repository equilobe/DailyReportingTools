using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AnotherJiraRestClient;
using AnotherJiraRestClient.JiraModel;

namespace JiraReporter.Model
{
    [Serializable]
    public class Issue
    {
        [XmlElement("key")]
        public string Key {get; set;}

        [XmlIgnore]
        public Uri Link { get; set; }
        [XmlIgnore]
        public string TimeLogged { get; set; }
        [XmlIgnore]
        public int TimeSpent { get; set; }
        [XmlIgnore]
        public string Resolution { get; set; }
        [XmlIgnore]
        public string Status { get; set; }
        [XmlIgnore]
        public string Assignee { get; set; }
        [XmlIgnore]
        public AnotherJiraRestClient.Priority Priority { get; set; }
        [XmlIgnore]
        public int RemainingEstimateSeconds { get; set; }
        [XmlIgnore]
        public string RemainingEstimate { get; set; }
        [XmlIgnore]
        public int TotalRemainingSeconds { get; set; }
        [XmlIgnore]
        public string TotalRemaining { get; set; }
        [XmlIgnore]
        public string Type { get; set; }
        [XmlIgnore]
        public bool SubTask { get; set; }
        [XmlIgnore]
        public Issue Parent { get; set; }
        [XmlIgnore]
        public string Label { get; set; }
        [XmlIgnore]
        public string ResolutionDate { get; set; }
        [XmlIgnore]
        public StatusCategory StatusCategory { get; set; }
        [XmlIgnore]
        public string Updated { get; set; }
        [XmlIgnore]
        public List<AnotherJiraRestClient.Subtask> Subtasks { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("entries", Type = typeof(Entries))]
        public List<Entries> Entries { get; set; }

        public Issue()
        {

        }

        public Issue(Issue issue)
        {
            if(issue.Assignee!=null)
                this.Assignee = issue.Assignee;
            if(issue.Entries!=null)
                this.Entries = issue.Entries;
            this.Key = issue.Key;
            if(issue.Label!=null)
                this.Label = issue.Label;
            this.Link = issue.Link;
            if (issue.Parent!=null)
                 this.Parent = issue.Parent;
            this.Priority = issue.Priority;
            this.RemainingEstimate = issue.RemainingEstimate;
            this.RemainingEstimateSeconds = issue.RemainingEstimateSeconds;
            this.TotalRemainingSeconds = issue.TotalRemainingSeconds;
            this.TotalRemaining = issue.TotalRemaining;
            if (issue.Resolution != null)
            {
                this.Resolution = issue.Resolution;
                this.ResolutionDate = issue.ResolutionDate;
            }
            this.Status = issue.Status;
            this.SubTask = issue.SubTask;
            this.Summary = issue.Summary;
            this.TimeLogged = issue.TimeLogged;
            this.TimeSpent = issue.TimeSpent;
            this.Type = issue.Type;
            this.StatusCategory = issue.StatusCategory;
            this.Updated = issue.Updated;
            if (issue.Subtasks!=null)
                this.Subtasks = issue.Subtasks;
        }
 
        public static void SetEntries(List<Entries> entries, Issue issue, List<Issue> issues)
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
                var newIssue = CreateNewIssue(issue);
                AddEntry(newIssue, entry);
                AddIssue(newIssue, issues);
            }
        }

        private static Issue IssueExists(Entries entry, List<Issue> issues, Issue issue)
        {
            return issues.Find(i => i.Entries.First().AuthorFullName == entry.AuthorFullName && i.Key == issue.Key);
        }

        private static Issue CreateNewIssue(Issue issue)
        {
            return new Issue
            {
                Key = issue.Key,
                Link = issue.Link,
                TimeLogged = issue.TimeLogged,
                TimeSpent = issue.TimeSpent,
                Summary = issue.Summary,
                Assignee = issue.Assignee,
                Priority = issue.Priority,
                RemainingEstimate = issue.RemainingEstimate,
                RemainingEstimateSeconds = issue.RemainingEstimateSeconds,
                Resolution = issue.Resolution,
                Status = issue.Status,
                SubTask = issue.SubTask,
                Type = issue.Type,
                Parent = issue.Parent,
                Label = issue.Label,
                ResolutionDate = issue.ResolutionDate,
                StatusCategory = issue.StatusCategory,
                Updated = issue.Updated,
                Subtasks = issue.Subtasks,
                TotalRemainingSeconds = issue.TotalRemainingSeconds,
                TotalRemaining = issue.TotalRemaining,
                Entries = new List<Entries>()
            };
        }

        private static void AddIssue(Issue issue, List<Issue> issues)
        {
            issues.Add(issue);
        }

        private static void AddEntry(Issue issue, Entries entry)
        {
            issue.Entries.Add(entry);
        }

        public static void SetIssues(Timesheet timesheet, Policy policy, Options options)
        {
            foreach (var issue in timesheet.Worklog.Issues)
            {
                var newIssue = new AnotherJiraRestClient.Issue();
                newIssue = GetIssue(issue.Key, policy);
                issue.SetIssue(policy, newIssue);
                if(issue.SubTask==true)
                   issue.GetParent(issue, policy);
            }
        }

        public void SetIssue(Policy policy, AnotherJiraRestClient.Issue newIssue)
        {
            
            this.Priority = newIssue.fields.priority;
            if (newIssue.fields.assignee!=null)
                this.Assignee = newIssue.fields.assignee.displayName;
            if (newIssue.fields.timetracking != null)
            {
                this.RemainingEstimate = newIssue.fields.timetracking.remainingEstimate;
                this.RemainingEstimateSeconds = newIssue.fields.timetracking.remainingEstimateSeconds;
            }
            if (newIssue.fields.resolution != null)
            {
                this.Resolution = newIssue.fields.resolution.name;
                this.ResolutionDate = newIssue.fields.resolutiondate;
            }
            this.Status = newIssue.fields.status.name;
            this.Type = newIssue.fields.issuetype.name;
            this.SubTask = newIssue.fields.issuetype.subtask;
            this.SetLabel(policy, newIssue);
            this.StatusCategory = newIssue.fields.status.statusCategory;
            this.Updated = newIssue.fields.updated;
            if (newIssue.fields.subtasks != null)
            {
                this.Subtasks = newIssue.fields.subtasks;
                this.TotalRemainingSeconds = newIssue.fields.aggregatetimeestimate;
            }

            if(this.Entries!=null)
               this.SetIssueTimeSpent();
            this.SetIssueTimeFormat();
            
            this.SetIssueLink(policy);
        }

        private void SetIssueTimeSpent()
        {
            foreach (var entry in this.Entries)
                this.TimeSpent += entry.TimeSpent;
        }

        public void SetIssueTimeFormat()
        {
                 this.TimeLogged = TimeFormatting.SetTimeFormat(this.TimeSpent);
                     if (this.Subtasks.Count>0)
                         this.TotalRemaining = TimeFormatting.SetParentTimeFormat(this.TotalRemainingSeconds);
                     if (this.RemainingEstimate == null)
                         this.RemainingEstimate = TimeFormatting.SetParentTimeFormat(this.RemainingEstimateSeconds);
        }

        public void SetIssueLink(Policy policy)
        {
            Uri baseLink = new Uri(policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            this.Link = new Uri(baseLink, this.Key);           
        }

        private void SetLabel(Policy policy, AnotherJiraRestClient.Issue issue)
        {
            foreach (var label in issue.fields.labels)
                if (label == policy.PermanentTaskLabel)
                    this.Label = label;
        }

        private static AnotherJiraRestClient.Issue GetIssue(string issueKey, Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issue = client.GetIssue(issueKey);
            return client.GetIssue(issueKey);
        }

        public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }

        private void GetParent(Issue issue, Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issueNew = client.GetIssue(issue.Key);
            issue.Parent = new Issue { Key = issueNew.fields.parent.key, Summary = issueNew.fields.parent.fields.summary };
            issue.Parent.SetIssueLink(policy);
        }
    }
}
