using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JiraReporter
{
    class AuthorsProcessing
    {
        public static List<Author> GetAuthors(Dictionary<TimesheetType, Timesheet> timesheetCollection, SprintTasks report, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<Commit> commits)
        {
            var authors = GetAuthorsDict(timesheetCollection[TimesheetType.ReportTimesheet]);
            var authorsNew = new List<Author>();
            var users = RestApiRequests.GetUsers(policy);
            if(policy.IgnoredAuthors != null && policy.IgnoredAuthors.Count > 0)
                users.RemoveAll(u => policy.IgnoredAuthors.Exists(a => a == u.displayName));

            foreach (var user in users)
            {
                if(authors.ContainsKey(user.displayName))
                    authorsNew.Add(new Author { Name = user.displayName, Issues = authors[user.displayName]});
                else
                    authorsNew.Add(new Author { Name = user.displayName });
                SetAuthor(report, authorsNew.Last(), policy, commits, options, timesheetCollection);
            }

            authorsNew.RemoveAll(AuthorIsEmpty);
            return authorsNew;
        }

        private static Dictionary<string, List<Issue>> GetAuthorsDict(Timesheet timesheet)
        {
            var authors = new Dictionary<string, List<Issue>>();
            foreach (var issue in timesheet.Worklog.Issues)
            {
                IssueAdapter.SetLoggedAuthor(issue, issue.Entries.First().AuthorFullName);
                Add(authors, issue.Entries.First().AuthorFullName, issue);
            }
               
            return authors;
        }

        private static void Add(Dictionary<string, List<Issue>> dict, string key, Issue issue)
        {
            if (dict.ContainsKey(key))
            {
                List<Issue> list = dict[key];
                list.Add(new Issue(issue));
            }
            else
            {
                List<Issue> list = new List<Issue>();
                list.Add(new Issue(issue));
                dict.Add(key, list);
            }
        }

        private static void SetAuthor(SprintTasks sprintTasks, Author author, SourceControlLogReporter.Model.Policy policy, List<Commit> commits, SourceControlLogReporter.Options options, Dictionary<TimesheetType,Timesheet> timesheetCollection)
        {
            author = OrderAuthorIssues(author);
            SetAuthorTimeSpent(author, timesheetCollection);
            SetAuthorTimeFormat(author);           
            GetAuthorCommits(policy, author, commits);
            author.Name = GetName(author.Name);
            SetAuthorsCommitsTasks(sprintTasks.UncompletedTasks, author);
            SetUnfinishedTasks(sprintTasks, author, policy);
            SetAuthorDayLogs(author, options);
            SetAuthorErrors(author);
            SetAuthorInitials(author);
            SetRemainingEstimate(author);
        }

        public static string GetName(string name)
        {
            string delimiter = "(\\[.*\\])";
            if(name!=null)
                name = Regex.Replace(name, delimiter, "");
            return name;
        }

        public static string GetFirstName(string name)
        {
            var names = name.Split(' ');
            return names[0];
        }

        public static string GetShortName(string name)
        {
            var names = name.Split(' ');
            return names[0] + " " + names[1][0] + ".";
        }

        public static void SetAuthorTimeFormat(Author author)
        {
                author.TimeLogged = TimeFormatting.SetTimeFormat((int)author.TimeSpent);
        }

        private static Author OrderAuthorIssues(Author author)
        {
                if(author.Issues!=null)
                    author.Issues = IssueAdapter.OrderIssues(author.Issues);
                return author;
        }

        private static void SetUnfinishedTasks(SprintTasks tasks, Author author, SourceControlLogReporter.Model.Policy policy)
        {
            SetAuthorInProgressTasks(tasks, author, policy);
            SetAuthorOpenTasks(tasks, author, policy);
        }

        private static void SetAuthorInProgressTasks(SprintTasks tasks, Author author, SourceControlLogReporter.Model.Policy policy)
        {
            author.InProgressTasks = new List<Issue>();
            author.InProgressTasks = GetAuthorTasks(tasks.InProgressTasks, author);
            TasksService.SetErrors(author.InProgressTasks, policy);
            if (author.InProgressTasks != null)
            {
                author.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(author.InProgressTasks);
                author.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.InProgressTasksTimeLeftSeconds);
            }
            author.InProgressTasksParents = TasksService.GetParentTasks(author.InProgressTasks, author);
            author.InProgressTasksParents = author.InProgressTasksParents.OrderBy(priority => priority.Priority.id).ToList(); 
        }

        private static void SetAuthorOpenTasks(SprintTasks tasks, Author author, SourceControlLogReporter.Model.Policy policy)
        {
            author.OpenTasks = new List<Issue>();
            author.OpenTasks = GetAuthorTasks(tasks.OpenTasks, author);
            TasksService.SetErrors(author.OpenTasks, policy);
            if (author.OpenTasks != null)
            {
                author.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(author.OpenTasks);
                author.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.OpenTasksTimeLeftSeconds);
            }
            author.OpenTasksParents = TasksService.GetParentTasks(author.OpenTasks, author);
            author.OpenTasksParents = author.OpenTasksParents.OrderBy(priority => priority.Priority.id).ToList();
        }

        private static void SetRemainingEstimate(Author author)
        {
            author.RemainingEstimateSeconds = author.InProgressTasksTimeLeftSeconds + author.OpenTasksTimeLeftSeconds;
            author.RemainingEstimateHours = author.RemainingEstimateSeconds / 3600;
        }

        private static List<Issue> GetAuthorTasks(List<Issue> tasks, Author author)
        {
            var unfinishedTasks = new List<Issue>();
            foreach (var task in tasks)
                if (task.Assignee == author.Name)
                {
                    unfinishedTasks.Add(task);
                    IssueAdapter.SetLoggedAuthor(unfinishedTasks.Last(), author.Name);
                    //if (unfinishedTasks.Last().ExistsInTimesheet == false)
                    //{
                    //    IssueAdapter.AdjustIssueCommits(unfinishedTasks.Last(), author.Commits);
                    //    if (unfinishedTasks.Last().Commits.Count > 0) 
                    //    {
                    //        if (author.Issues == null)
                    //            author.Issues = new List<Issue>();
                    //        unfinishedTasks.Last().ExistsInTimesheet = true;
                    //        author.Issues.Add(new Issue(unfinishedTasks.Last()));
                    //    }
                    //}
                }
            return unfinishedTasks;
        } 

        public static void SetAuthorsCommitsTasks(List<Issue> tasks, Author author)
        {
            foreach(var task in tasks)
            {
                if((author.Issues!=null && author.Issues.Exists(i=>i.Key == task.Key) == false) || author.Issues == null)
                {
                    var issue = new Issue(task);
                    issue.Commits = null;
                    IssueAdapter.AdjustIssueCommits(issue, author.Commits);
                    if (issue.Commits.Count > 0)
                    {
                        if (author.Issues == null)
                            author.Issues = new List<Issue>();
                        issue.ExistsInTimesheet = true;
                        IssueAdapter.SetLoggedAuthor(issue, author.Name);
                        author.Issues.Add(issue);
                    }
                }
            }
        }
       
        private static bool AuthorIsEmpty(Author author)
        {
            if (author.InProgressTasks.Count == 0 && author.OpenTasks.Count == 0 && author.DayLogs.Count==0)
                return true;
            return false;
        }

        public static void GetAuthorCommits(SourceControlLogReporter.Model.Policy policy, Author author, List<Commit> commits)
        {            
            var find = new List<Commit>();             
            author.Commits = new List<Commit>();
            if(policy.Users.ContainsKey(author.Name))
                if(policy.Users[author.Name]!="")
                  find = commits.FindAll(commit => commit.Entry.Author == policy.Users[author.Name]);
            author.Commits = find;
            SetCommitsLink(commits, policy);
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

        private static void SetCommitsLink(List<Commit> commits, SourceControlLogReporter.Model.Policy policy)
        {
            if(commits!=null)
              foreach (var commit in commits)
                  if (commit.Entry.Link == null && policy.SourceControl.CommitUrl != null)
                      commit.Entry.Link = policy.SourceControl.CommitUrl + commit.Entry.Revision;
        }

        private static void SetAuthorInitials(Author author)
        {
            string name = author.Name;
            var nameParts = name.Split(' ');
            string initials = "";
            foreach (var part in nameParts)
                initials += Regex.Match(part, "[A-Z]");
            author.Initials = initials;
        }

        private static void SetAuthorTimeSpent(Author author, Dictionary<TimesheetType,Timesheet> timesheetCollection)
        {
            author.TimeSpent = timesheetCollection[TimesheetType.ReportTimesheet].GetTimesheetSecondsWorkedAuthor(author);
            author.TimeSpentCurrentMonthSeconds = timesheetCollection[TimesheetType.MonthTimesheet].GetTimesheetSecondsWorkedAuthor(author);
            if (timesheetCollection.ContainsKey(TimesheetType.SprintTimesheet))
                if(timesheetCollection[TimesheetType.SprintTimesheet] != null)
                 author.TimeSpentCurrentSprintSeconds = timesheetCollection[TimesheetType.SprintTimesheet].GetTimesheetSecondsWorkedAuthor(author);
        }

        public static void SetAuthorErrors(Author author)
        {
            var inProgressTasksErrors = new List<Error>();
            if (author.InProgressTasks != null && author.InProgressTasks.Count > 0)
                inProgressTasksErrors = author.InProgressTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
            var openTasksErrors = new List<Error>();
            if(author.OpenTasks!=null && author.OpenTasks.Count>0)
                openTasksErrors = author.OpenTasks.Where(e => e.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();

            if(inProgressTasksErrors.Count>0 || openTasksErrors.Count>0)
            {
                author.Errors = new List<Error>();
                author.Errors = author.Errors.Concat(inProgressTasksErrors).ToList();
                author.Errors = author.Errors.Concat(openTasksErrors).ToList();
            }
        }   
    }
}
