﻿using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JiraReporter
{
    class AuthorsProcessing
    {
        public static List<Author> GetAuthors(Timesheet timesheet, SprintTasks report, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var log = SourceControlProcessor.GetSourceControlLog(policy, options);
            var authors = GetAuthorsDict(timesheet);
            var authorsNew = new List<Author>();
            var users = RestApiRequests.GetUsers(policy);
            var commits = SourceControlProcessor.GetCommits(log);            

            foreach (var user in users)
            {
                if(authors.ContainsKey(user.displayName))
                    authorsNew.Add(new Author { Name = user.displayName, Issues = authors[user.displayName]});
                else
                    authorsNew.Add(new Author { Name = user.displayName });
                SetAuthor(report, authorsNew.Last(), policy, commits, options);
            }

            authorsNew.RemoveAll(AuthorIsEmpty);
            return authorsNew;
        }

        private static Dictionary<string, List<Issue>> GetAuthorsDict(Timesheet timesheet)
        {
            var authors = new Dictionary<string, List<Issue>>();
            foreach (var issue in timesheet.Worklog.Issues)
            {
                Add(authors, issue.Entries.First().AuthorFullName, issue);
                issue.LoggedAuthor = issue.Entries.First().AuthorFullName;
                issue.LoggedAuthor = SetName(issue.LoggedAuthor);
            }
               
            return authors;

        }

        private static void Add(Dictionary<string, List<Issue>> dict, string key, Issue issue)
        {
            if (dict.ContainsKey(key))
            {
                List<Issue> list = dict[key];
                list.Add(issue);
            }
            else
            {
                List<Issue> list = new List<Issue>();
                list.Add(issue);
                dict.Add(key, list);
            }
        }

        private static void SetAuthor(SprintTasks sprint, Author author, SourceControlLogReporter.Model.Policy policy, List<Commit> commits, SourceControlLogReporter.Options options)
        {
            author = OrderAuthorIssues(author);
            SetAuthorTimeSpent(author);
            SetAuthorTimeFormat(author);           
            SetAuthorCommits(policy, author, commits);
            author.Name = SetName(author.Name);
            SetUnfinishedTasks(sprint, author);
            SetAuthorDayLogs(author, options);
        }

        public static string SetName(string name)
        {
            string delimiter = "(\\[.*\\])";
            if(name!=null)
                name = Regex.Replace(name, delimiter, "");
            return name;
        }

        private static void SetAuthorTimeSpent(Author author)
        {
            if(author.Issues!=null)
              foreach (var issue in author.Issues)
                author.TimeSpent += issue.TimeSpent;

            author.TimeLogged = author.TimeSpent.ToString();
        }

        public static void SetAuthorTimeFormat(Author author)
        {
                author.TimeLogged = TimeFormatting.SetTimeFormat(author.TimeSpent);
        }

        private static Author OrderAuthorIssues(Author author)
        {
                if(author.Issues!=null)
                    author.Issues = IssueAdapter.OrderIssues(author.Issues);
                return author;
        }

        private static void SetUnfinishedTasks(SprintTasks report, Author author)
        {
            author.InProgressTasks = GetAuthorTasks(report.InProgressTasks, author);
            if (author.InProgressTasks != null)
            {
                author.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(author.InProgressTasks);
                author.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.InProgressTasksTimeLeftSeconds);
            }

            author.OpenTasks = GetAuthorTasks(report.OpenTasks, author);
            if (author.OpenTasks != null)
            {
                author.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(author.OpenTasks);
                author.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.OpenTasksTimeLeftSeconds);
            }
        }

        private static List<Issue> GetAuthorTasks(List<Issue> tasks, Author author)
        {
            var unfinishedTasks = new List<Issue>();
            foreach (var task in tasks)
                if (task.Assignee == author.Name)
                {
                    unfinishedTasks.Add(task);
                    unfinishedTasks.Last().LoggedAuthor = author.Name;
                    if (unfinishedTasks.Last().ExistsInTimesheet == false)
                    {
                        IssueAdapter.AdjustIssueCommits(unfinishedTasks.Last(), author.Commits);
                        if (unfinishedTasks.Last().Commits.Count > 0) 
                        {
                            if (author.Issues == null)
                                author.Issues = new List<Issue>();
                            author.Issues.Add(unfinishedTasks.Last());
                        }
                           
                    }
                }
            return unfinishedTasks;
        } 
       
        private static bool AuthorIsEmpty(Author author)
        {
            if (author.InProgressTasks.Count == 0 && author.OpenTasks.Count == 0 && author.DayLogs.Count==0)
                return true;
            return false;
        }

        public static void SetAuthorCommits(SourceControlLogReporter.Model.Policy policy, Author author, List<Commit> commits)
        {            
            var find = new List<Commit>();             
            author.Commits = new List<Commit>();
            if(policy.Users.ContainsKey(author.Name))
                if(policy.Users[author.Name]!="")
                  find = commits.FindAll(commit => commit.Entry.Author == policy.Users[author.Name]);
            author.Commits = find;
           // IssueAdapter.AdjustIssueCommits(author);           
        }       
        public static void SetAuthorDayLogs(Author author, SourceControlLogReporter.Options options)
        {
            author.DayLogs = new List<DayLog>();
            foreach (var day in options.ReportDates)
                author.DayLogs.Add(new DayLog(author, day, options));
            author.DayLogs = author.DayLogs.OrderBy(d => d.Date).ToList();
            author.DayLogs.RemoveAll(d => d.Commits.Count == 0 && d.Issues == null);
        }

        public static List<Commit> GetDayLogCommits(Author author, DateTime date)
        {
            var commits = new List<Commit>();
            if (author.Commits != null)
                commits = author.Commits.FindAll(c => c.Entry.Date.ToOriginalTimeZone() >= date && c.Entry.Date.ToOriginalTimeZone() < date.AddDays(1));
            return commits;
        }
    }
}
