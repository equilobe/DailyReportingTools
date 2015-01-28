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
    class AuthorLoader
    {
        SprintTasks _sprintIssues;
        SourceControlLogReporter.Model.Policy _policy;
        List<Commit> _commits;
        SourceControlLogReporter.Options _options;
        List<PullRequest> _pullRequests;
        Sprint _sprint;
        Author _currentAuthor;

        public AuthorLoader(SourceControlLogReporter.Options options, SourceControlLogReporter.Model.Policy policy, Sprint sprint, SprintTasks sprintIssues, List<Commit> commits, List<PullRequest> pullRequests)
        {
            this._sprintIssues = sprintIssues;
            this._commits = commits;
            this._policy = policy;
            this._options = options;
            this._pullRequests = pullRequests;
            this._sprint = sprint;
        }

        public List<Author> GetAuthors()
        {
            var authors = RestApiRequests.GetUsers(_policy)
                            .Where(UserIsNotIgnored)
                            .Select(u => new Author(u))
                            .ToList();

            authors.ForEach(SetAuthorAdvancedProperties);          
            authors.RemoveAll(AuthorIsEmpty);

            return authors;
        }

        private bool UserIsNotIgnored(JiraUser u)
        {
            var userOptions = _policy.UserOptions.Find(user => user.JiraUserKey == u.key);
            if (userOptions == null)
                return true;

            return !userOptions.Ignored;
        }



        private void SetAuthorAdvancedProperties(Author a)
        {
            this._currentAuthor = a;
            SetTimesheets();
            LoadTimesheetIssueDetails();
            SetIssues();
            OrderIssues();
            CalculateTimeSpent();
            SetCommits();
            CleanName();

            AddCommitIssuesNotInTimesheet();

            SetUncompletedTasks();
            SetAuthorDayLogs();
            SetAuthorErrors();
            SetAuthorInitials();
            SetRemainingEstimate();
            SetImage();
        }

        private void CleanName()
        {
            _currentAuthor.Name = AuthorHelpers.GetCleanName(_currentAuthor.Name);
        }

        private void SetTimesheets()
        {
            _currentAuthor.CurrentTimesheet = RestApiRequests.GetTimesheet(_policy, _options.FromDate, _options.ToDate.AddDays(-1), _currentAuthor.Username);
            if (_sprint != null)
                _currentAuthor.SprintTimesheet = RestApiRequests.GetTimesheet(_policy, _sprint.StartDate.ToOriginalTimeZone(), _sprint.EndDate.ToOriginalTimeZone(), _currentAuthor.Username);
            _currentAuthor.MonthTimesheet = RestApiRequests.GetTimesheet(_policy, DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1), _currentAuthor.Username);
        }


        private void LoadTimesheetIssueDetails()
        {
            var timesheetService = new TimesheetService();
            timesheetService.SetTimesheetIssues(_currentAuthor.CurrentTimesheet, _policy, _pullRequests);
            timesheetService.SetTimesheetIssues(_currentAuthor.SprintTimesheet, _policy, _pullRequests);
            timesheetService.SetTimesheetIssues(_currentAuthor.MonthTimesheet, _policy, _pullRequests);
        }

        private void SetIssues()
        {
            if (_currentAuthor.CurrentTimesheet != null && _currentAuthor.CurrentTimesheet.Worklog.Issues != null)
                _currentAuthor.Issues = _currentAuthor.CurrentTimesheet.Worklog.Issues;
            _currentAuthor.Issues.ForEach(issue => IssueAdapter.SetLoggedAuthor(issue, _currentAuthor.Name));
        }

        private void OrderIssues()
        {
            if (_currentAuthor.Issues != null)
                _currentAuthor.Issues = IssueAdapter.OrderIssues(_currentAuthor.Issues);
        }

        private void CalculateTimeSpent()
        {
            _currentAuthor.TimeSpent = _currentAuthor.CurrentTimesheet.GetTimesheetSecondsWorked();
            _currentAuthor.TimeSpentCurrentMonthSeconds = _currentAuthor.MonthTimesheet.GetTimesheetSecondsWorked();
            if (_currentAuthor.SprintTimesheet != null)
                _currentAuthor.TimeSpentCurrentSprintSeconds = _currentAuthor.SprintTimesheet.GetTimesheetSecondsWorked();
        }

        private void SetCommits()
        {                      
            _currentAuthor.Commits = GetCommits();
        }

        private List<Commit> GetCommits()
        {
            if (_policy.SourceControlOptions == null)
                return new List<Commit>();

            if (!_policy.Users.ContainsKey(_currentAuthor.Username))
                return new List<Commit>();

            return _policy.Users[_currentAuthor.Username]
                    .SelectMany(sourceControlUser => _commits.FindAll(commit => commit.Entry.Author == sourceControlUser))
                    .ToList();
        }

        private void SetCommitsLink()
        {
            if (_commits == null)
                return;

            foreach (var commit in _commits)
                if (commit.Entry.Link == null && _policy.SourceControlOptions.CommitUrl != null)
                    commit.Entry.Link = _policy.SourceControlOptions.CommitUrl + commit.Entry.Revision;
        }

        private void AddCommitIssuesNotInTimesheet(List<Issue> tasks)
        {
            foreach (var task in tasks)
            {
                if ((_currentAuthor.Issues != null && _currentAuthor.Issues.Exists(i => i.Key == task.Key) == false) || _currentAuthor.Issues == null)
                {
                    var issue = new Issue(task);
                    issue.Commits = null;
                    IssueAdapter.AdjustIssueCommits(issue, _currentAuthor.Commits);
                    if (issue.Commits.Count > 0)
                    {
                        if (_currentAuthor.Issues == null)
                            _currentAuthor.Issues = new List<Issue>();
                        issue.ExistsInTimesheet = true;
                        IssueAdapter.SetLoggedAuthor(issue, _currentAuthor.Name);
                        _currentAuthor.Issues.Add(new Issue(issue));
                    }
                }
            }
        }

        private void AddCommitIssuesNotInTimesheet()
        {
            AddCommitIssuesNotInTimesheet(_sprintIssues.UncompletedTasks);

            foreach (var listOfTasks in _sprintIssues.CompletedTasks.Values)
                AddCommitIssuesNotInTimesheet(listOfTasks);
        }


        private void SetUncompletedTasks()
        {
            SetAuthorInProgressTasks();
            SetAuthorOpenTasks();
        }

        private void SetAuthorInProgressTasks()
        {
            _currentAuthor.InProgressTasks = new List<Issue>();
            _currentAuthor.InProgressTasks = GetAuthorTasks(_sprintIssues.InProgressTasks);
            TasksService.SetErrors(_currentAuthor.InProgressTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(_currentAuthor.InProgressTasks, _currentAuthor.Issues);
            if (_currentAuthor.InProgressTasks != null)
            {
                _currentAuthor.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_currentAuthor.InProgressTasks);
                _currentAuthor.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(_currentAuthor.InProgressTasksTimeLeftSeconds);
            }
            _currentAuthor.InProgressTasksParents = TasksService.GetParentTasks(_currentAuthor.InProgressTasks, _currentAuthor);
            _currentAuthor.InProgressTasksParents = _currentAuthor.InProgressTasksParents.OrderBy(priority => priority.Priority.id).ToList();
        }


        private bool AuthorIsEmpty(Author author)
        {
            if (author.InProgressTasks.Count == 0 && author.OpenTasks.Count == 0 && author.DayLogs.Count == 0)
                return true;
            return false;
        }

        private void SetAuthorDayLogs()
        {
            _currentAuthor.DayLogs = new List<DayLog>();
            foreach (var day in _options.ReportDates)
                _currentAuthor.DayLogs.Add(new DayLog(_currentAuthor, day, _options));
            _currentAuthor.DayLogs = _currentAuthor.DayLogs.OrderBy(d => d.Date).ToList();
            _currentAuthor.DayLogs.RemoveAll(d => d.Commits.Count == 0 && d.Issues == null);
        }


        private void SetAuthorInitials()
        {
            string name = _currentAuthor.Name;
            var nameParts = name.Split(' ');
            string initials = "";
            foreach (var part in nameParts)
                initials += Regex.Match(part, "[A-Z]");
            _currentAuthor.Initials = initials;
        }


        private void SetAuthorErrors()
        {
            var inProgressTasksErrors = new List<Error>();
            if (_currentAuthor.InProgressTasks != null && _currentAuthor.InProgressTasks.Count > 0)
                inProgressTasksErrors = _currentAuthor.InProgressTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
            var openTasksErrors = new List<Error>();
            if (_currentAuthor.OpenTasks != null && _currentAuthor.OpenTasks.Count > 0)
                openTasksErrors = _currentAuthor.OpenTasks.Where(e => e.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();

            if (inProgressTasksErrors.Count > 0 || openTasksErrors.Count > 0)
            {
                _currentAuthor.Errors = new List<Error>();
                _currentAuthor.Errors = _currentAuthor.Errors.Concat(inProgressTasksErrors).ToList();
                _currentAuthor.Errors = _currentAuthor.Errors.Concat(openTasksErrors).ToList();
            }
        }


        private void SetAuthorOpenTasks()
        {
            _currentAuthor.OpenTasks = new List<Issue>();
            _currentAuthor.OpenTasks = GetAuthorTasks(_sprintIssues.OpenTasks);
            TasksService.SetErrors(_currentAuthor.OpenTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(_currentAuthor.OpenTasks, _currentAuthor.Issues);
            if (_currentAuthor.OpenTasks != null)
            {
                _currentAuthor.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_currentAuthor.OpenTasks);
                _currentAuthor.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(_currentAuthor.OpenTasksTimeLeftSeconds);
            }
            _currentAuthor.OpenTasksParents = TasksService.GetParentTasks(_currentAuthor.OpenTasks, _currentAuthor);
            _currentAuthor.OpenTasksParents = _currentAuthor.OpenTasksParents.OrderBy(priority => priority.Priority.id).ToList();
        }

        private List<Issue> GetAuthorTasks(List<Issue> tasks)
        {
            var unfinishedTasks = new List<Issue>();
            foreach (var task in tasks)
                if (task.Assignee == _currentAuthor.Name)
                {
                    unfinishedTasks.Add(task);
                    IssueAdapter.SetLoggedAuthor(unfinishedTasks.Last(), _currentAuthor.Name);
                }
            return unfinishedTasks;
        }

        private void SetImage()
        {
            _currentAuthor.Image = WebDownloads.ImageFromURL(_currentAuthor.AvatarLink.OriginalString, _policy.Username, _policy.Password);
        }

        private void SetRemainingEstimate()
        {
            _currentAuthor.RemainingEstimateSeconds = _currentAuthor.InProgressTasksTimeLeftSeconds + _currentAuthor.OpenTasksTimeLeftSeconds;
            _currentAuthor.RemainingEstimateHours = _currentAuthor.RemainingEstimateSeconds / 3600;
        }
    }
}
