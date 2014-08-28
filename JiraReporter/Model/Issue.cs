using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter.Model
{
    [Serializable]
    public class Issue
    {
        [XmlElement("key")]
        public string Key;

        [XmlIgnore]
        public Uri Link;

        public string TimeLogged;

        public int TimeSpent;

        [XmlElement("summary")]
        public string Summary;

        [XmlElement("entries", Type = typeof(Entries))]
        public List<Entries> Entries;

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
                Summary = issue.Summary,
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

        public static void SetIssues(Timesheet timesheet)
        {
            foreach (var issue in timesheet.Worklog.Issues)
            {
                SetIssueTimeSpent(issue);
                SetIssueLink(issue);
            }
        }

        public static void SetIssueTimeSpent(Issue issue)
        {
            foreach (var entry in issue.Entries)
                issue.TimeSpent += entry.TimeSpent;
        }

        public static void SetIssueTimeFormat(Issue issue)
        {
            issue.TimeLogged = Timesheet.SetTimeFormat(issue.TimeSpent);
        }

        private static void SetIssueLink(Issue issue)
        {
            Uri baseLink=new Uri("https://equilobe.atlassian.net/browse/");
            issue.Link = new Uri(baseLink, issue.Key);           
        }

        public static List<Issue> OrderIssues(List<Issue> issues)
        {
            return issues.OrderByDescending(i => i.TimeSpent).ToList();
        }
    }
}
