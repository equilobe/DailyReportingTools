using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
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
        public IDataService DataService { get; set; }

        SprintTasks _reportTasks { get { return _context.ReportTasks; } }
        JiraPolicy _policy { get { return _context.Policy; } }
        List<JiraCommit> _commits { get { return _context.Commits; } }
        JiraOptions _options { get { return _context.Options; } }
        Sprint _sprint { get { return _context.Sprint; } }
        JiraAuthor _currentAuthor;
        JiraReport _context;
        int _currentTasksCount;
        DateTime _startOfMonth;

        public AuthorLoader(JiraReport context)
        {
            this._context = context;
            _startOfMonth = GetStartOfMonth();
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

        private DateTime GetStartOfMonth()
        {
            if (_options.FromDate.Month != _options.ToDate.AddDays(-1).Month)
                return _options.ToDate.AddDays(-1).StartOfMonth();

            return _options.FromDate.StartOfMonth();
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
            a.HasSprint = _context.HasSprint;
            SetIssuesSearchUrl();
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
            SetOverrideEmail();
            SetAuthorIsEmpty();
            SetImage();
        }

        private void SetIssuesSearchUrl()
        {
            if (!_context.HasSprint)
                return;

            _currentAuthor.IssueSearchUrl = new Uri(_context.Settings.BaseUrl + "/issues/?jql=assignee='" + _currentAuthor.Username + "' and project=" + _context.ProjectKey + " and sprint=" + _context.Sprint.id + " and status != 'Done'");
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
                _currentAuthor.SprintIssues = GetAuthorsTimesheetIssues(_sprint.StartDate.ToOriginalTimeZone(_context.OffsetFromUtc).Date, _options.ToDate);
            _currentAuthor.MonthIssues = GetAuthorsTimesheetIssues(_startOfMonth, _options.ToDate);
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
                fullIssue.SubtasksDetailed.ForEach(i=>i.Entries.RemoveAll(e => e.AuthorFullName != _currentAuthor.Name || e.StartDate < fromDate || e.StartDate > toDate));
                fullIssue.SubtasksDetailed.RemoveAll(s => s.Entries.IsEmpty());
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
                        AdjustSubtasks(issue);
                        issue.SubtasksDetailed.RemoveAll(s => s.Entries.IsEmpty());
                        _currentAuthor.Issues.Add(new IssueDetailed(issue));
                    }
                }
            }
        }

        private void AdjustSubtasks(IssueDetailed issue)
        {
            foreach (var subtask in issue.SubtasksDetailed)
            {
                subtask.Entries.RemoveAll(e => e.AuthorFullName != _currentAuthor.Name || e.StartDate < _context.FromDate || e.StartDate > _context.ToDate);
                IssueAdapter.TimeSpentFromEntries(subtask);
                IssueAdapter.SetTimeFormat(subtask);
            }
        }

        private void AddCommitIssuesNotInTimesheet()
        {
            AddCommitIssuesNotInTimesheet(_reportTasks.CompletedTasksAll);

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
            SetAuthorRemainingTasksSection();
        }

        private void SetAuthorInProgressTasks()
        {
            var inProgressTasks = new List<IssueDetailed>();
            inProgressTasks = GetAuthorTasks(_reportTasks.InProgressTasks);
            _currentAuthor.InProgressTasksCount = inProgressTasks.Count();
            TaskLoader.SetErrors(inProgressTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(inProgressTasks, _currentAuthor.Issues);
            if (inProgressTasks != null)
            {
                _currentAuthor.Timing.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(inProgressTasks);
                _currentAuthor.Timing.InProgressTasksTimeLeftString = _currentAuthor.Timing.InProgressTasksTimeLeftSeconds.SetTimeFormat8Hour();
            }

            _currentAuthor.InProgressTasks = new AuthorTasks
            {
                Issues = TaskLoader.GetParentTasks(inProgressTasks, _currentAuthor),
                AuthorName = _currentAuthor.Name
            };

            _currentAuthor.InProgressTasks.Issues = _currentAuthor.InProgressTasks.Issues.OrderBy(priority => priority.Priority.id).ToList();
        }

        private void SetAuthorRemainingTasksSection()
        {
            _currentAuthor.Timing.RemainingTasksTimeLeftSeconds = _currentAuthor.Timing.InProgressTasksTimeLeftSeconds + _currentAuthor.Timing.OpenTasksTimeLeftSeconds;
            _currentAuthor.Timing.RemainingTasksTimeLeftString = _currentAuthor.Timing.RemainingTasksTimeLeftSeconds.SetTimeFormat8Hour();
            _currentTasksCount = 0;

            _currentAuthor.RemainingTasks = new AuthorTasks();
            _currentAuthor.RemainingTasks.AuthorName = _currentAuthor.Name;
            _currentAuthor.RemainingTasks.Issues = new List<IssueDetailed>();

            AddToRemainingTasksList(_currentAuthor.InProgressTasks.Issues);
            AddToRemainingTasksList(_currentAuthor.OpenTasks.Issues);

            SetUncompletedTasksAdditionalCount();
        }

        private void AddToRemainingTasksList(List<IssueDetailed> tasks)
        {
            foreach (var issue in tasks)
            {
                if (_currentTasksCount >= 3)
                    return;

                var newIssue = new IssueDetailed();
                if (issue.Assignee == _currentAuthor.Name)
                    _currentTasksCount++;

                issue.CopyPropertiesOnObjects(newIssue);
                newIssue.Subtasks = new List<Subtask>();
                newIssue.SubtasksDetailed = new List<IssueDetailed>();
                _currentAuthor.RemainingTasks.Issues.Add(newIssue);

                if (!issue.SubtasksDetailed.IsEmpty())
                {
                    foreach (var subtask in issue.SubtasksDetailed)
                    {
                        if (subtask.Assignee != _currentAuthor.Name || subtask.Resolution != null)
                            continue;

                        _currentTasksCount++;
                        newIssue.SubtasksDetailed.Add(subtask);
                        if (_currentTasksCount == 3)
                            return;
                    }
                }
            }
        }

        private void SetUncompletedTasksAdditionalCount()
        {
            _currentAuthor.RemainingTasksCount = _currentAuthor.InProgressTasksCount + _currentAuthor.OpenTasksCount;
            _currentAuthor.AdditionalUncompletedTasksCount = _currentAuthor.RemainingTasksCount - _currentTasksCount;
        }


        private void SetAuthorIsEmpty()
        {
            var hasInProgress = _currentAuthor.InProgressTasks != null && !_currentAuthor.InProgressTasks.Issues.IsEmpty();
            var hasOpenTasks = _currentAuthor.OpenTasks != null && !_currentAuthor.OpenTasks.Issues.IsEmpty();
            var hasDayLogs = !_currentAuthor.DayLogs.IsEmpty();
            var hasIssues = (!_currentAuthor.MonthIssues.IsEmpty()
                || !_currentAuthor.SprintIssues.IsEmpty());

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
            _currentAuthor.DayLogs.RemoveAll(d => d.Commits.Count == 0 && (d.Issues.IsEmpty()));
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

            if (!_currentAuthor.RemainingTasks.Issues.IsEmpty())
            {
                var errors = _currentAuthor.RemainingTasks.Issues.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
                if (errors.Count > 0)
                    _currentAuthor.Errors = errors;
            }
        }


        private void SetAuthorOpenTasks()
        {
            var openTasks = new List<IssueDetailed>();
            openTasks = GetAuthorTasks(_reportTasks.OpenTasks);
            _currentAuthor.OpenTasksCount = openTasks.Count;
            TaskLoader.SetErrors(openTasks, _policy);
            IssueAdapter.SetIssuesExistInTimesheet(openTasks, _currentAuthor.Issues);
            if (openTasks != null)
            {
                _currentAuthor.Timing.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(openTasks);
                _currentAuthor.Timing.OpenTasksTimeLeftString = _currentAuthor.Timing.OpenTasksTimeLeftSeconds.SetTimeFormat8Hour();
            }

            _currentAuthor.OpenTasks = new AuthorTasks
            {
                Issues = TaskLoader.GetParentTasks(openTasks, _currentAuthor),
                AuthorName = _currentAuthor.Name
            };

            _currentAuthor.OpenTasks.Issues = _currentAuthor.OpenTasks.Issues.OrderBy(priority => priority.Priority.id).ToList();
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
            if (_currentAuthor.IsEmpty)
                return;

            var image = JiraService.GetUserAvatar(_context.JiraRequestContext, _currentAuthor.JiraAvatarLink.OriginalString);
            SetUserAvatar(image);
        }

        private void SetRemainingEstimate()
        {
            if (!_context.HasSprint)
                return;

            _currentAuthor.Timing.TotalRemainingSeconds = _currentAuthor.Timing.InProgressTasksTimeLeftSeconds + _currentAuthor.Timing.OpenTasksTimeLeftSeconds;
            _currentAuthor.Timing.TotalRemainingHours = (double)_currentAuthor.Timing.TotalRemainingSeconds / 3600;
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

        private void SetUserAvatar(byte[] image)
        {
            var context = new UserImageContext
            {
                Username = _currentAuthor.Username,
                InstanceId = _context.Settings.InstalledInstanceId,
                Image = image
            };

            DataService.AddUserImage(context);
            var key = DataService.GetUserImageKey(_currentAuthor.Username);
            SetUserAvatarLink(key);
        }

        private void SetUserAvatarLink(string imageKey)
        {
            _currentAuthor.ReportAvatarLink = new Uri(ConfigurationService.GetWebBaseUrl() + "/avatar/image/" + imageKey);
        }

    }
}
