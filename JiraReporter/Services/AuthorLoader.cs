using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace JiraReporter.Services
{
    class AuthorLoader
    {
        public IJiraService JiraService { get; set; }
        public IEncryptionService EncryptionService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }

        SprintTasks _reportTasks { get { return _context.ReportTasks; } }
        JiraPolicy _policy { get { return _context.Policy; } }
        List<JiraCommit> _commits { get { return _context.Commits; } }
        JiraOptions _options { get { return _context.Options; } }
        Sprint _sprint { get { return _context.Sprint; } }
        JiraAuthor _currentAuthor;
        JiraReport _context;

        public AuthorLoader(JiraReport context)
        {
            this._context = context;
        }

        public List<JiraAuthor> GetAuthors()
        {
            var authors = JiraService.GetUsers(_context.JiraRequestContext, _context.ProjectKey)
                            .Where(UserIsNotIgnored)
                            .Select(u => new JiraAuthor(u))
                            .ToList();

            SetProjectLead(authors);
            authors.ForEach(SetAuthorAdvancedProperties);
            authors.RemoveAll(a => a.IsProjectLead == false && a.IsEmpty);

            var individualReportService = new IndividualReportInfoService();
            individualReportService.SetIndividualDraftInfo(authors, _context);

            return authors;
        }

        public JiraAuthor CreateAuthorByKey(JiraReport context)
        {
            var draftInfoService = new IndividualReportInfoService();
            var draft = draftInfoService.GetIndividualDraftInfo(context);
            var user = JiraService.GetUser(context.JiraRequestContext, draft.Username);
            var author = new JiraAuthor(user);
            SetAuthorAdvancedProperties(author);
            author.IndividualDraftInfo = draft;

            return author;
        }

        private bool UserIsNotIgnored(JiraUser u)
        {
            var userJiraOptions = _policy.UserOptions.Find(user => user.JiraUserKey == u.key);
            if (userJiraOptions == null)
                return true;

            return !userJiraOptions.Ignored;
        }



        public void SetAuthorAdvancedProperties(JiraAuthor a)
        {
            this._currentAuthor = a;
            SetTimesheets();
            SetCommits();
            OrderIssues();
            CalculateTimeSpent();
            SetName();

            AddCommitIssuesNotInTimesheet();

            SetUncompletedTasks();
            SetAuthorDayLogs();
            SetAuthorErrors();
            SetRemainingEstimate();
            SetImage();
            SetAvatarId();
            SetOverrideEmail();
            SetAuthorIsEmpty();
        }

        private void SetName()
        {
            _currentAuthor.Name = AuthorHelpers.GetCleanName(_currentAuthor.Name);
            _currentAuthor.ShortName = AuthorHelpers.GetShortName(_currentAuthor.Name);
            _currentAuthor.FirstName = AuthorHelpers.GetFirstName(_currentAuthor.Name);
            SetAuthorInitials();
        }

        private void SetTimesheets()
        {
            _currentAuthor.Issues = GetAuthorsTimesheetIssues(_options.FromDate, _options.ToDate);
            if (_sprint != null)
                _currentAuthor.SprintIssues = GetAuthorsTimesheetIssues(_sprint.StartDate.ToOriginalTimeZone(_context.OffsetFromUtc), _options.ToDate);
            _currentAuthor.MonthIssues = GetAuthorsTimesheetIssues(_options.FromDate.StartOfMonth(), _options.ToDate);
        }

        private List<IssueDetailed> GetAuthorsTimesheetIssues(DateTime fromDate, DateTime toDate)
        {
            var timesheetContext = GetTimesheetContext(fromDate, toDate);

            var issues = JiraService.GetTimesheetForUser(timesheetContext);
            var issueProcessor = new IssueProcessor(_context) { JiraService = JiraService };
            var completeIssues = new List<IssueDetailed>();
            foreach (var issue in issues)
            {
                var fullIssue = IssueAdapter.GetBasicIssue(issue);
                fullIssue.Entries.RemoveAll(e => e.AuthorFullName != _currentAuthor.Name || e.StartDate < fromDate || e.StartDate > toDate);
                issueProcessor.SetIssue(fullIssue, issue);
                IssueAdapter.SetLoggedAuthor(fullIssue, _currentAuthor.Name);
                completeIssues.Add(fullIssue);
            }

            return completeIssues;
        }

        private TimesheetContext GetTimesheetContext(DateTime fromDate, DateTime toDate)
        {
            var timesheetContext = new TimesheetContext
            {
                RequestContext = _context.JiraRequestContext,
                ProjectKey = _context.ProjectKey,
                TargetUser = _currentAuthor.Username,
                StartDate = fromDate,
                EndDate = toDate
            };
            return timesheetContext;
        }

        private void OrderIssues()
        {
            if (_currentAuthor.Issues != null)
                _currentAuthor.Issues = IssueAdapter.OrderIssues(_currentAuthor.Issues);
        }

        private void CalculateTimeSpent()
        {
            _currentAuthor.Timing = new Timing();
            _currentAuthor.Timing.TotalTimeSeconds = GetSecondsWorked(_currentAuthor.Issues);
            _currentAuthor.Timing.TotalTimeString = _currentAuthor.Timing.TotalTimeSeconds.SetTimeFormat8Hour();
            _currentAuthor.Timing.TimeLogged = _currentAuthor.Timing.TotalTimeSeconds.SetTimeFormat();
            _currentAuthor.Timing.MonthSecondsWorked = GetSecondsWorked(_currentAuthor.MonthIssues);
            if (_currentAuthor.SprintIssues != null)
                _currentAuthor.Timing.SprintSecondsWorked = GetSecondsWorked(_currentAuthor.SprintIssues);
        }

        private int GetSecondsWorked(List<IssueDetailed> issues)
        {
            return issues.Sum(i => i.Entries.Sum(t => t.TimeSpent));
        }

        private void SetCommits()
        {
            _currentAuthor.Commits = GetCommits();
        }

        private List<JiraCommit> GetCommits()
        {
            if (_policy.SourceControlOptions == null)
                return new List<JiraCommit>();

            if (!_policy.Users.ContainsKey(_currentAuthor.UserKey))
                return new List<JiraCommit>();

            return _policy.Users[_currentAuthor.UserKey]
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

        private void AddCommitIssuesNotInTimesheet(List<IssueDetailed> tasks)
        {
            foreach (var task in tasks)
            {
                if ((_currentAuthor.Issues != null && _currentAuthor.Issues.Exists(i => i.Key == task.Key) == false) || _currentAuthor.Issues == null)
                {
                    var issue = new IssueDetailed(task);
                    issue.Commits = null;
                    IssueAdapter.AdjustIssueCommits(issue, _currentAuthor.Commits);
                    if (issue.Commits.Count > 0)
                    {
                        if (_currentAuthor.Issues == null)
                            _currentAuthor.Issues = new List<IssueDetailed>();
                        issue.ExistsInTimesheet = true;
                        IssueAdapter.SetLoggedAuthor(issue, _currentAuthor.Name);
                        _currentAuthor.Issues.Add(new IssueDetailed(issue));
                    }
                }
            }
        }

        private void AddCommitIssuesNotInTimesheet()
        {
            foreach (var listOfTasks in _reportTasks.CompletedTasks.Values)
                AddCommitIssuesNotInTimesheet(listOfTasks);

            if (!_context.HasSprint)
                return;

            AddCommitIssuesNotInTimesheet(_reportTasks.UncompletedTasks);
        }


        private void SetUncompletedTasks()
        {
            if (!_context.HasSprint)
                return;

            SetAuthorInProgressTasks();
            SetAuthorOpenTasks();
        }

        private void SetAuthorInProgressTasks()
        {
            _currentAuthor.InProgressTasks = new List<IssueDetailed>();
            _currentAuthor.InProgressTasks = GetAuthorTasks(_reportTasks.InProgressTasks);
            TaskLoader.SetErrors(_currentAuthor.InProgressTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(_currentAuthor.InProgressTasks, _currentAuthor.Issues);
            if (_currentAuthor.InProgressTasks != null)
            {
                _currentAuthor.Timing.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_currentAuthor.InProgressTasks);
                _currentAuthor.Timing.InProgressTasksTimeLeftString = _currentAuthor.Timing.InProgressTasksTimeLeftSeconds.SetTimeFormat8Hour();
            }
            _currentAuthor.InProgressTasksParents = TaskLoader.GetParentTasks(_currentAuthor.InProgressTasks, _currentAuthor);
            _currentAuthor.InProgressTasksParents = _currentAuthor.InProgressTasksParents.OrderBy(priority => priority.Priority.id).ToList();
        }


        private void SetAuthorIsEmpty()
        {
            var hasInProgress = _currentAuthor.InProgressTasks != null && _currentAuthor.InProgressTasks.Count > 0;
            var hasOpenTasks = _currentAuthor.OpenTasks != null && _currentAuthor.OpenTasks.Count > 0;
            var hasDayLogs = _currentAuthor.DayLogs != null && _currentAuthor.DayLogs.Count > 0;
            var hasIssues = (_currentAuthor.MonthIssues != null && _currentAuthor.MonthIssues.Count > 0)
                || (_currentAuthor.SprintIssues != null && _currentAuthor.SprintIssues.Count > 0);

            if (hasInProgress || hasOpenTasks)
                _currentAuthor.HasAssignedIssues = true;

            if (hasDayLogs)
                _currentAuthor.HasDayLogs = true;

            if (!_currentAuthor.HasDayLogs && !_currentAuthor.HasAssignedIssues && !hasIssues)
                _currentAuthor.IsEmpty = true;
        }

        private void SetAuthorDayLogs()
        {
            _currentAuthor.DayLogs = new List<JiraDayLog>();
            foreach (var day in _options.ReportDates)
                _currentAuthor.DayLogs.Add(DayLogLoader.CreateDayLog(_currentAuthor, day, _context));
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
            if (!_context.HasSprint)
                return;

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
            _currentAuthor.OpenTasks = new List<IssueDetailed>();
            _currentAuthor.OpenTasks = GetAuthorTasks(_reportTasks.OpenTasks);
            TaskLoader.SetErrors(_currentAuthor.OpenTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(_currentAuthor.OpenTasks, _currentAuthor.Issues);
            if (_currentAuthor.OpenTasks != null)
            {
                _currentAuthor.Timing.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_currentAuthor.OpenTasks);
                _currentAuthor.Timing.OpenTasksTimeLeftString = _currentAuthor.Timing.OpenTasksTimeLeftSeconds.SetTimeFormat8Hour();
            }
            _currentAuthor.OpenTasksParents = TaskLoader.GetParentTasks(_currentAuthor.OpenTasks, _currentAuthor);
            _currentAuthor.OpenTasksParents = _currentAuthor.OpenTasksParents.OrderBy(priority => priority.Priority.id).ToList();
        }

        private List<IssueDetailed> GetAuthorTasks(List<IssueDetailed> tasks)
        {
            var unfinishedTasks = new List<IssueDetailed>();
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
            _currentAuthor.Image = GetImageFromURL(_currentAuthor.AvatarLink.OriginalString);
        }

        private void SetRemainingEstimate()
        {
            if (!_context.HasSprint)
                return;

            _currentAuthor.Timing.TotalRemainingSeconds = _currentAuthor.Timing.InProgressTasksTimeLeftSeconds + _currentAuthor.Timing.OpenTasksTimeLeftSeconds;
            _currentAuthor.Timing.TotalRemainingHours = (double)_currentAuthor.Timing.TotalRemainingSeconds / 3600;
    //        _currentAuthor.Timing.TotalRemainingString = _currentAuthor.Timing.TotalRemainingSeconds.SetTimeFormat8Hour();
        }

        private void SetOverrideEmail()
        {
            var author = _policy.UserOptions.Find(u => _currentAuthor.Username == u.JiraUserKey && u.EmailOverride != null);
            if (author != null)
                _currentAuthor.EmailAdress = author.EmailOverride;
        }

        private void SetProjectLead(List<JiraAuthor> authors)
        {
            var lead = authors.Find(a => a.UserKey == _context.ProjectManager);
            if (lead == null)
            {
                var projectManager = GetProjectLead(_context.ProjectManager);
                authors.Add(projectManager);
            }
            else
                lead.IsProjectLead = true;
        }

        private JiraAuthor GetProjectLead(string username)
        {
            var lead = JiraService.GetUser(_context.JiraRequestContext, username);
            var projectManager = new JiraAuthor(lead);
            projectManager.IsProjectLead = true;

            return projectManager;
        }

        private Image GetImageFromURL(string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "image/png");
            webClient.Authorize(_context.JiraRequestContext, UrlExtensions.GetRelativeUrl(url), ConfigurationService.GetAddonKey());

            var imageData = webClient.DownloadData(url);

            MemoryStream stream = new MemoryStream(imageData);
            var img = Image.FromStream(stream);
            stream.Close();

            return img;
        }

        private void SetAvatarId()
        {
            var avatar = _currentAuthor.AvatarLink.OriginalString;
            _currentAuthor.AvatarId = avatar.Substring(avatar.LastIndexOf("avatarId="));
        }
    }
}
