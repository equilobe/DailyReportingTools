using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RestSharp;

namespace JiraReporter
{
    class AuthorsProcessing
    {
        public static List<Author> GetAuthors(Dictionary<TimesheetType, Timesheet> timesheetCollection, SprintTasks report, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<Commit> commits)
        {
            var reportAuthors = GetAllAuthors(timesheetCollection);

            var authors = new List<Author>();
            var users = RestApiRequests.GetUsers(policy);
            users = RemoveIgnoredUsers(users, policy);

            foreach (var user in users)
            {
                var author = CreateAuthor(user);
                if (reportAuthors.Contains(user.displayName))
                {
                    var issues = GetAuthorIssuesFromTimesheet(timesheetCollection[TimesheetType.ReportTimesheet], user.displayName);
                    author.Issues = issues;
                }

                SetAuthor(report, author, policy, commits, options, timesheetCollection);
                authors.Add(author);
            }

            authors.RemoveAll(a => reportAuthors.Exists(t => t == a.Name) == false && AuthorIsEmpty(a));
            return authors;
        }

        private static List<JiraUser> RemoveIgnoredUsers(List<JiraUser> users, SourceControlLogReporter.Model.Policy policy)
        {
            try
            {
                users.RemoveAll(u => policy.UserOptions.Find(user => user.JiraUserKey == u.key).Ignored == true);
                return users;
            }
            catch(Exception)
            {
                return users;
            }
        }

        private static Author CreateAuthor(JiraUser user)
        {
            var author = new Author
            {
                Name = user.displayName,
                Username = user.key,
                AvatarLink = user.avatarUrls.Big,
                EmailAdress = user.emailAddress
            };
            return author;
        }

        private static List<string> GetAllAuthors(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            var sprintAuthors = GetTimesheetCollectionAuthors(timesheetCollection, TimesheetType.SprintTimesheet);
            var loggedAuthors = GetTimesheetCollectionAuthors(timesheetCollection, TimesheetType.ReportTimesheet);
            var monthAuthors = GetTimesheetCollectionAuthors(timesheetCollection, TimesheetType.MonthTimesheet);
            var reportAuthors = sprintAuthors.Concat(loggedAuthors)
                                             .Concat(monthAuthors)
                                             .Distinct()
                                             .ToList();
            return reportAuthors;
        }

        private static List<Issue> GetAuthorIssuesFromTimesheet(Timesheet timesheet, string author)
        {
            var issues = timesheet.Worklog.Issues.Where(i => i.Entries.First().AuthorFullName == author).ToList();
            if (issues != null && issues.Count > 0)
                foreach (var issue in issues)
                    IssueAdapter.SetLoggedAuthor(issue, author);
            return issues;
        }

        private static List<string> GetTimesheetCollectionAuthors(Dictionary<TimesheetType, Timesheet> timesheetCollection, TimesheetType key)
        {
            if (timesheetCollection.ContainsKey(key) && timesheetCollection[key] != null)
                return GetAuthorsFromTimesheet(timesheetCollection[key]);
            else
                return new List<string>();
        }

        private static void SetAuthor(SprintTasks sprintTasks, Author author, SourceControlLogReporter.Model.Policy policy, List<Commit> commits, SourceControlLogReporter.Options options, Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            author = OrderAuthorIssues(author);
            SetAuthorTimeSpent(author, timesheetCollection);
            SetAuthorTimeFormat(author);
            GetAuthorCommits(policy, author, commits);
            author.Name = GetCleanName(author.Name);
            SetCommitsAllTasks(author, sprintTasks);
            SetUncompletedTasks(sprintTasks, author, policy);
            SetAuthorDayLogs(author, options);
            SetAuthorErrors(author);
            SetAuthorInitials(author);
            SetRemainingEstimate(author);
            SetImage(author, policy);
        }

        public static string GetCleanName(string name)
        {
            string delimiter = "(\\[.*\\])";
            if (name != null)
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
            if (names.Count() > 1)
                return names[0] + " " + names[1][0] + ".";
            else
                return names[0];
        }

        public static void SetAuthorTimeFormat(Author author)
        {
            author.TimeLogged = TimeFormatting.SetTimeFormat((int)author.TimeSpent);
        }

        private static Author OrderAuthorIssues(Author author)
        {
            if (author.Issues != null)
                author.Issues = IssueAdapter.OrderIssues(author.Issues);
            return author;
        }

        private static void SetUncompletedTasks(SprintTasks tasks, Author author, SourceControlLogReporter.Model.Policy policy)
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
                }
            return unfinishedTasks;
        }

        public static void SetAuthorsCommitsTasks(List<Issue> tasks, Author author)
        {
            foreach (var task in tasks)
            {
                if ((author.Issues != null && author.Issues.Exists(i => i.Key == task.Key) == false) || author.Issues == null)
                {
                    var issue = new Issue(task);
                    issue.Commits = null;
                    IssueAdapter.AdjustIssueCommits(issue, author.Commits);
                    if (issue.Commits.Count > 0)
                    {
                        if (author.Issues == null)
                            author.Issues = new List<Issue>();
                        issue.ExistsInTimesheet = true;
                        IssueAdapter.ChangeLoggedAuthor(issue, author.Name);
                        author.Issues.Add(new Issue(issue));
                    }
                }
            }
        }

        public static void SetCommitsAllTasks(Author author, SprintTasks tasks)
        {
            SetAuthorsCommitsTasks(tasks.UncompletedTasks, author);
            foreach (var listOfTasks in tasks.CompletedTasks.Values)
                SetAuthorsCommitsTasks(listOfTasks, author);
        }

        private static bool AuthorIsEmpty(Author author)
        {
            if (author.InProgressTasks.Count == 0 && author.OpenTasks.Count == 0 && author.DayLogs.Count == 0)
                return true;
            return false;
        }

        public static void GetAuthorCommits(SourceControlLogReporter.Model.Policy policy, Author author, List<Commit> commits)
        {
            var find = new List<Commit>();
            author.Commits = new List<Commit>();
            author.Commits = GetSourceControlUsersCommits(policy, author, commits);
            SetCommitsLink(commits, policy);
            // IssueAdapter.AdjustIssueCommits(author);           
        }

        private static List<Commit> GetSourceControlUsersCommits(SourceControlLogReporter.Model.Policy policy, Author author, List<Commit> commits)
        {
            var commitsList = new List<Commit>();
            if (policy.Users.ContainsKey(author.Username))
                if (policy.Users[author.Username].Count > 0)
                    foreach (var sourceControlUser in policy.Users[author.Username])
                    {
                        var userCommits = commits.FindAll(commit => commit.Entry.Author == sourceControlUser);
                        commitsList = commitsList.Concat(userCommits).ToList();
                    }
            return commitsList;
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
            if (commits != null)
                foreach (var commit in commits)
                    if (commit.Entry.Link == null && policy.SourceControlOptions.CommitUrl != null)
                        commit.Entry.Link = policy.SourceControlOptions.CommitUrl + commit.Entry.Revision;
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

        private static void SetAuthorTimeSpent(Author author, Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            author.TimeSpent = timesheetCollection[TimesheetType.ReportTimesheet].GetTimesheetSecondsWorkedAuthor(author);
            author.TimeSpentCurrentMonthSeconds = timesheetCollection[TimesheetType.MonthTimesheet].GetTimesheetSecondsWorkedAuthor(author);
            if (timesheetCollection.ContainsKey(TimesheetType.SprintTimesheet))
                if (timesheetCollection[TimesheetType.SprintTimesheet] != null)
                    author.TimeSpentCurrentSprintSeconds = timesheetCollection[TimesheetType.SprintTimesheet].GetTimesheetSecondsWorkedAuthor(author);
        }

        public static void SetAuthorErrors(Author author)
        {
            var inProgressTasksErrors = new List<Error>();
            if (author.InProgressTasks != null && author.InProgressTasks.Count > 0)
                inProgressTasksErrors = author.InProgressTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
            var openTasksErrors = new List<Error>();
            if (author.OpenTasks != null && author.OpenTasks.Count > 0)
                openTasksErrors = author.OpenTasks.Where(e => e.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();

            if (inProgressTasksErrors.Count > 0 || openTasksErrors.Count > 0)
            {
                author.Errors = new List<Error>();
                author.Errors = author.Errors.Concat(inProgressTasksErrors).ToList();
                author.Errors = author.Errors.Concat(openTasksErrors).ToList();
            }
        }

        public static bool AuthorExistsInTimesheet(Author author, Timesheet timesheet)
        {
            if (timesheet != null && timesheet.Worklog.Issues != null)
                return timesheet.Worklog.Issues.Exists(i => i.Assignee == author.Name);
            return false;
        }

        public static List<string> GetAuthorsFromTimesheet(Timesheet timesheet)
        {
            var authors = new List<string>();
            if (timesheet != null && timesheet.Worklog != null)
                authors = timesheet.Worklog.Issues.SelectMany(i => i.Entries.Select(e => e.AuthorFullName)).Distinct().ToList();
            return authors;
        }

        public static void SetAuthorAverageWorkPerDay(Author author, int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            if (monthWorkedDays == 0)
                author.MonthWorkedPerDay = 0;
            else
                author.MonthWorkedPerDay = author.TimeSpentCurrentMonthSeconds / monthWorkedDays;
            if (sprintWorkedDays == 0)
                author.SprintWorkedPerDay = 0;
            else
                author.SprintWorkedPerDay = author.TimeSpentCurrentSprintSeconds / sprintWorkedDays;
            if (reportWorkingDays == 0)
                author.TimeLoggedPerDayAverage = 0;
            else
                author.TimeLoggedPerDayAverage = author.TimeSpent / reportWorkingDays;
        }

        public static double GetAuthorMaxAverage(Author author)
        {
            var max = Math.Max(author.TimeLoggedPerDayAverage, author.SprintWorkedPerDay);
            max = Math.Max(max, author.MonthWorkedPerDay);
            return max;
        }

        public static void SetAuthorWorkSummaryWidths(Author author, int maxWidth, int maxValue)
        {
            author.SprintChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.SprintWorkedPerDay / 3600));
            author.MonthChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.MonthWorkedPerDay / 3600));
            author.DayChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.TimeLoggedPerDayAverage / 3600));
        }

        private static void SetImage(Author author, SourceControlLogReporter.Model.Policy policy)
        {
            author.Image = WebDownloads.ImageFromURL(author.AvatarLink.OriginalString, policy.Username, policy.Password);
        }

        public static List<Author> GetActiveAuthors(List<Author> authors)
        {
            var activeAuthors = new List<Author>();
            foreach (var author in authors)
                if (IsActive(author))
                    activeAuthors.Add(author);
            return activeAuthors;
        }

        private static bool IsActive(Author author)
        {
            if (author.Issues != null && author.Issues.Count > 0)
                return true;
            if (author.InProgressTasks != null && author.InProgressTasks.Count > 0)
                return true;
            if (author.OpenTasks != null && author.OpenTasks.Count > 0)
                return true;
            if (author.Commits != null && author.Commits.Count > 0)
                return true;

            return false;
        }
    }
}
