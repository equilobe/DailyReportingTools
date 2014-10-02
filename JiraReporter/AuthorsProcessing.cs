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
        public static List<Author> GetAuthors(Timesheet timesheet, SprintTasks report, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var authors = GetAuthorsDict(timesheet);
            var authorsNew = new List<Author>();
            var users = RestApiRequests.GetUsers(policy);
            var commits = SourceControlProcessor.GetSourceControlCommits(policy, options);

            foreach (var user in users)
            {
                if(authors.ContainsKey(user.displayName))
                    authorsNew.Add(new Author { Name = user.displayName, Issues = authors[user.displayName]});
                else
                    authorsNew.Add(new Author { Name = user.displayName });
                SetAuthor(report, authorsNew.Last(), policy, commits, pullRequests);
            }

            authorsNew.RemoveAll(AuthorIsEmpty);
            return authorsNew;
        }

        private static Dictionary<string, List<Issue>> GetAuthorsDict(Timesheet timesheet)
        {
            var authors = new Dictionary<string, List<Issue>>();
            foreach (var issue in timesheet.Worklog.Issues)
                Add(authors, issue.Entries.First().AuthorFullName, issue);
            return authors;

        }

        private static void Add(Dictionary<string, List<Issue>> dict, string key, Issue value)
        {
            if (dict.ContainsKey(key))
            {
                List<Issue> list = dict[key];
                list.Add(value);
            }
            else
            {
                List<Issue> list = new List<Issue>();
                list.Add(value);
                dict.Add(key, list);
            }
        }

        private static void SetAuthor(SprintTasks sprint, Author author, SvnLogReporter.Model.Policy policy, List<Commit> commits, List<PullRequest> pullRequests)
        {
            author = OrderAuthorIssues(author);
            SetAuthorTimeSpent(author);
            SetAuthorTimeFormat(author);
            SetUnfinishedTasks(sprint, author);
            SetAuthorCommits(policy, author, commits);
            SetAuthorName(author.Name);
            SetAuthorPullRequestsCount(author, pullRequests);
        }

        public static void SetAuthorName(string author)
        {
            string delimiter = "(\\[.*\\])";
            if(author!=null)
                author = Regex.Replace(author, delimiter, "");
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
            //    author.InProgressTasksCount = author.InProgressTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
                author.InProgressTasksTimeLeftSeconds = TasksService.GetTasksTimeLeftSeconds(author.InProgressTasks);
                author.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.InProgressTasksTimeLeftSeconds);
            }

            author.OpenTasks = GetAuthorTasks(report.OpenTasks, author);
            if (author.OpenTasks != null)
            {
              //  author.OpenTasksCount = author.OpenTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
                author.OpenTasksTimeLeftSeconds = TasksService.GetTasksTimeLeftSeconds(author.OpenTasks);
                author.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.OpenTasksTimeLeftSeconds);
            }
        }

        private static List<Task> GetAuthorTasks(List<Task> tasks, Author author)
        {
            var unfinishedTasks = new List<Task>();
            foreach (var task in tasks)
                if (task.Issue.Assignee == author.Name)
                    unfinishedTasks.Add(task);
            return unfinishedTasks;
        } 
       
        private static bool AuthorIsEmpty(Author author)
        {
            if (author.Issues == null && author.InProgressTasksCount == 0 && author.OpenTasksCount == 0 && author.Commits.Count==0)
                return true;
            return false;
        }

        public static void SetAuthorCommits(SvnLogReporter.Model.Policy policy, Author author, List<Commit> commits)
        {            
            var find = new List<Commit>();             
            author.Commits = new List<Commit>();
            if(policy.Users.ContainsKey(author.Name))
                if(policy.Users[author.Name]!="")
                  find = commits.FindAll(commit => commit.Entry.Author == policy.Users[author.Name]);
            author.Commits = find;
            IssueAdapter.AdjustIssueCommits(author);           
        }       
 
        public static void SetAuthorPullRequestsCount(Author author, List<PullRequest> pullRequests)
        {
            if (author.Issues != null)
                if (author.Issues.Count > 0)
                    foreach (var issue in author.Issues)
                        author.PullRequestsCount += issue.PullRequests.Count;
        }
    }
}
