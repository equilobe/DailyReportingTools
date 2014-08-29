﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AnotherJiraRestClient;

namespace JiraReporter.Model
{
    [Serializable]
    public class Issue
    {
        [XmlElement("key")]
        public string Key {get; set;}

        [XmlIgnore]
        public Uri Link { get; set; }

        public string TimeLogged { get; set; }
        public int TimeSpent { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
        public string Assignee { get; set; }
        public string Priority { get; set; }
        public int RemainingEstimateSeconds { get; set; }
        public string RemainingEstimate { get; set; }
        public string Type { get; set; }
        public bool SubTask { get; set; }
        public Issue Parent { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("entries", Type = typeof(Entries))]
        public List<Entries> Entries { get; set; }
 
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

        public static void SetIssues(Timesheet timesheet, Policy policy)
        {
            foreach (var issue in timesheet.Worklog.Issues)
                issue.SetIssue(policy);
        }

        private void SetIssue(Policy policy)
        {
            var newIssue = new AnotherJiraRestClient.Issue();
            newIssue = GetIssue(this.Key, policy);
            this.Priority = newIssue.fields.priority.name;
            if (newIssue.fields.assignee!=null)
                this.Assignee = newIssue.fields.assignee.displayName;
            this.RemainingEstimate = newIssue.fields.timetracking.remainingEstimate;
            this.RemainingEstimateSeconds = newIssue.fields.timetracking.remainingEstimateSeconds;
            if (newIssue.fields.resolution != null)
                this.Resolution = newIssue.fields.resolution.name;
            this.Status = newIssue.fields.status.name;
            this.Type = newIssue.fields.issuetype.name;
            this.SubTask = newIssue.fields.issuetype.subtask;

            this.SetIssueTimeSpent();
            this.SetIssueTimeFormat();
            this.SetIssueLink(policy);
        }

        private void SetIssueTimeSpent()
        {
            foreach (var entry in this.Entries)
                this.TimeSpent += entry.TimeSpent;
        }

        private void SetIssueTimeFormat()
        {
                 this.TimeLogged = Timesheet.SetTimeFormat(this.TimeSpent);
        }

        private void SetIssueLink(Policy policy)
        {
            Uri baseLink = new Uri(policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            this.Link = new Uri(baseLink, this.Key);           
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
    }
}
