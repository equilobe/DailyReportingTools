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
        public static List<Author> GetAuthors(SprintTasks sprintTasks, SourceControlLogReporter.Model.Policy policy, List<Commit> commits, SourceControlLogReporter.Options options, List<PullRequest> pullRequests, Sprint sprint)
        {
            var users = RestApiRequests.GetUsers(policy);
            users = RemoveIgnoredUsers(users, policy);
            var authors = AuthorsWithTimesheet(users, options, policy, sprint);

            foreach (var author in authors)
                SetAuthor(sprintTasks, author, policy, commits, options, pullRequests);

            authors.RemoveAll(a => AuthorIsEmpty(a));

            GetIndividualDraftInfo(authors, policy);

            return authors;
        }

        private static List<Author> AuthorsWithTimesheet(List<JiraUser> users, SourceControlLogReporter.Options options, SourceControlLogReporter.Model.Policy policy, Sprint sprint)
        {
            var authors = new List<Author>();
            foreach (var user in users)
                authors.Add(GetAuthorWithTimesheet(user, options, policy, sprint));

            return authors;
        }

        private static Author GetAuthorWithTimesheet(JiraUser user, SourceControlLogReporter.Options options, SourceControlLogReporter.Model.Policy policy, Sprint sprint)
        {
            var author = new Author(user);
            author.CurrentTimesheet = RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1), author.Username);
            if(sprint != null)
                author.SprintTimesheet = RestApiRequests.GetTimesheet(policy, sprint.StartDate.ToOriginalTimeZone(), sprint.EndDate.ToOriginalTimeZone(), author.Username);
            author.MonthTimesheet = RestApiRequests.GetTimesheet(policy, DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1), author.Username);

            return author;
        }

        private static List<JiraUser> RemoveIgnoredUsers(List<JiraUser> users, SourceControlLogReporter.Model.Policy policy)
        {
            try
            {
                users.RemoveAll(u => policy.UserOptions.Find(user => user.JiraUserKey == u.key).Ignored == true);
                return users;
            }
            catch (Exception)
            {
                return users;
            }
        }

        private static void GetAuthorIssuesFromTimesheet(Author author)
        {
            if (author.CurrentTimesheet != null && author.CurrentTimesheet.Worklog.Issues != null)
                author.Issues = author.CurrentTimesheet.Worklog.Issues;
        }

        private static void SetAuthor(SprintTasks sprintTasks, Author author, SourceControlLogReporter.Model.Policy policy, List<Commit> commits, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            SetAuthorTimesheet(author, policy, options, pullRequests);
            GetAuthorIssuesFromTimesheet(author);
            OrderAuthorIssues(author);
            SetAuthorTimeSpent(author);
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

        private static void OrderAuthorIssues(Author author)
        {
            if (author.Issues != null)
                author.Issues = IssueAdapter.OrderIssues(author.Issues);
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
            IssueAdapter.SetIssuesExistInTimesheet(author.InProgressTasks, author.Issues);
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
            IssueAdapter.SetIssuesExistInTimesheet(author.OpenTasks, author.Issues);
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
            if(policy.SourceControlOptions != null)
            {
                author.Commits = GetSourceControlUsersCommits(policy, author, commits);
                SetCommitsLink(commits, policy);
            }         
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

        private static void SetAuthorTimeSpent(Author author)
        {
            author.TimeSpent = author.CurrentTimesheet.GetTimesheetSecondsWorked();
            author.TimeSpentCurrentMonthSeconds = author.MonthTimesheet.GetTimesheetSecondsWorked();
            if(author.SprintTimesheet != null)
                author.TimeSpentCurrentSprintSeconds = author.SprintTimesheet.GetTimesheetSecondsWorked();
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

        private static void SetAuthorTimesheet(Author author, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var timesheetService = new TimesheetService();
            timesheetService.SetTimesheetIssues(author.CurrentTimesheet, policy, options, pullRequests);
            timesheetService.SetTimesheetIssues(author.SprintTimesheet, policy, options, pullRequests);
            timesheetService.SetTimesheetIssues(author.MonthTimesheet, policy, options, pullRequests);
        }

        private static void GetIndividualDraftInfo(List<Author> authors, SourceControlLogReporter.Model.Policy policy)
        {
            if(!policy.AdvancedOptions.NoIndividualDraft)
            {
                var individualDrafts = new List<SourceControlLogReporter.Model.IndividualDraftInfo>();
                foreach (var author in authors)
                {
                    var individualDraft = GenerateIndividualDraftInfo(author, policy);
                    individualDrafts.Add(individualDraft);
                    author.IndividualDraftInfo = individualDraft;
                }

                policy.GeneratedProperties.IndividualDrafts = individualDrafts;
            }
        }

        private static SourceControlLogReporter.Model.IndividualDraftInfo GenerateIndividualDraftInfo(Author author, SourceControlLogReporter.Model.Policy policy)
        {
            var individualDraft = new SourceControlLogReporter.Model.IndividualDraftInfo
            {
                Name = author.Name,
                Username = author.Username,
                UserKey = RandomGenerator.RandomString(10)
            };
            SetIndividualUrls(individualDraft, policy);

            return individualDraft;
        }

        private static void SetIndividualUrls(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, SourceControlLogReporter.Model.Policy policy)
        {
            individualDraft.ConfirmationDraftUrl = SetUrl(individualDraft, policy.IndividualDraftConfirmationUrl);
            individualDraft.ResendDraftUrl = SetUrl(individualDraft, policy.ResendIndividualDraft);
        }

        private static Uri SetUrl(SourceControlLogReporter.Model.IndividualDraftInfo individualDraft, Uri baseUrl)
        {
            var url = string.Format("draftKey={0}", individualDraft.UserKey);

            return new Uri(baseUrl + "&" + url);
        }
    }
}
