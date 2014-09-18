﻿using System;
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
        public int OriginalEstimateSeconds { get; set; }
        [XmlIgnore]
        public int OriginalEstimateSecondsTotal { get; set; }
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
        [XmlIgnore]
        public List<Issue> SubtasksIssues { get; set; }
        [XmlIgnore]
        public bool ExistsInTimesheet { get; set; }
        [XmlIgnore]
        public DateTime Created { get; set; }

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
            this.OriginalEstimateSeconds = issue.OriginalEstimateSeconds;
            this.OriginalEstimateSecondsTotal = issue.OriginalEstimateSecondsTotal;
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
            this.Created = issue.Created;
            this.Updated = issue.Updated;
            this.ExistsInTimesheet = issue.ExistsInTimesheet;
            if (issue.Subtasks != null)
            {
                this.Subtasks = issue.Subtasks;
                this.SubtasksIssues = issue.SubtasksIssues;
            }
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
                var newIssue = new Issue(issue) { Entries = new List<Entries>() };
                AddEntry(newIssue, entry);
                AddIssue(newIssue, issues);
            }
        }

        private static Issue IssueExists(Entries entry, List<Issue> issues, Issue issue)
        {
            return issues.Find(i => i.Entries.First().AuthorFullName == entry.AuthorFullName && i.Key == issue.Key);
        }

        private bool IssueExistsTimesheet(List<Issue> issues)
        {
            if (issues.Exists(i => i.Key == this.Key))
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

        public static void SetIssues(Timesheet timesheet, Policy policy, Options options)
        {
            foreach (var issue in timesheet.Worklog.Issues)
            {
                var newIssue = new AnotherJiraRestClient.Issue();
                newIssue = GetIssue(issue.Key, policy);
                issue.SetIssue(policy, newIssue, timesheet);
                if (issue.Subtasks != null)
                    issue.SetSubtasksIssues(policy, timesheet);
            }
        }

        public void SetIssue(Policy policy, AnotherJiraRestClient.Issue newIssue, Timesheet timesheet)
        {
            
            this.Priority = newIssue.fields.priority;
            if (newIssue.fields.assignee!=null)
                this.Assignee = newIssue.fields.assignee.displayName;
            if (newIssue.fields.timetracking != null)
            {
                this.RemainingEstimate = newIssue.fields.timetracking.remainingEstimate;
                this.RemainingEstimateSeconds = newIssue.fields.timetracking.remainingEstimateSeconds;
            }
            this.OriginalEstimateSecondsTotal = newIssue.fields.aggregatetimeoriginalestimate;
            this.OriginalEstimateSeconds = newIssue.fields.timeoriginalestimate;
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
            this.Created = Convert.ToDateTime(newIssue.fields.created);
            this.Updated = newIssue.fields.updated;
            if (newIssue.fields.subtasks!=null)
            {
                this.Subtasks = newIssue.fields.subtasks;
                this.TotalRemainingSeconds = newIssue.fields.aggregatetimeestimate;
            }

            if (this.Entries != null)
                this.SetIssueTimeSpent();
            else
                this.TimeSpent = newIssue.fields.timespent;
            this.SetIssueTimeFormat();
            this.SetIssueExists(timesheet.Worklog.Issues);
            if (this.SubTask == true)
                this.SetParent(newIssue, policy);
            
            this.SetIssueLink(policy);
        }

        public void SetSubtasksIssues(Policy policy, Timesheet timesheet)
        {
            var issue = new AnotherJiraRestClient.Issue();
            this.SubtasksIssues = new List<Issue>();
            foreach(var task in Subtasks)
            {
                issue = GetIssue(task.key, policy);
                this.SubtasksIssues.Add(new Issue { Key = task.key, Summary=issue.fields.summary });               
                this.SubtasksIssues.Last().SetIssue(policy, issue, timesheet);
            }
        }

        private void SetIssueTimeSpent()
        {
            foreach (var entry in this.Entries)
                this.TimeSpent += entry.TimeSpent;
        }

        public void SetIssueTimeFormat()
        {
                 this.TimeLogged = TimeFormatting.SetTimeFormat(this.TimeSpent);
                     if (this.Subtasks!=null)
                         this.TotalRemaining = TimeFormatting.SetTimeFormat8Hour(this.TotalRemainingSeconds);
                     if (this.RemainingEstimate == null)
                         this.RemainingEstimate = TimeFormatting.SetTimeFormat8Hour(this.RemainingEstimateSeconds);
        }

        private void SetIssueLink(Policy policy)
        {
            Uri baseLink = new Uri(policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            this.Link = new Uri(baseLink, this.Key);           
        }

        private void SetIssueExists(List<Issue> issues)
        {
            if (IssueExistsTimesheet(issues) == true)
                this.ExistsInTimesheet = true;
            else
                this.ExistsInTimesheet = false;
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

        private void SetParent(AnotherJiraRestClient.Issue issue, Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);           
            this.Parent = new Issue { Key = issue.fields.parent.key, Summary = issue.fields.parent.fields.summary };
            this.Parent.SetIssueLink(policy);
        }
    }
}
