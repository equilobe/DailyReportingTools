using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Policy;
using JiraReporter.Helpers;
using Equilobe.DailyReport.Models.Interfaces;

namespace JiraReporter
{
    class IssueProcessor
    {
        public IJiraService JiraService { get; set; }

        JiraPolicy _policy
        {
            get
            {
                return _context.Policy;
            }
        }
        List<JiraPullRequest> _pullRequests;
        IssueDetailed _currentIssue;
        JiraIssue _currentJiraIssue;
        JiraReport _context;

        public IssueProcessor(JiraReport context)
        {
            this._context = context;
            this._pullRequests = context.PullRequests;
        }

        public void SetIssues(List<IssueDetailed> issues)
        {
            foreach (var issue in issues)
            {
                var jiraIssue = new JiraIssue();
                jiraIssue = JiraService.GetIssue(_context.JiraRequestContext, issue.Key);
                SetIssue(issue, jiraIssue);
            }
        }

        public void SetIssue(IssueDetailed issue, JiraIssue jiraIssue)
        {
            SetGenericIssue(issue, jiraIssue);
            if (issue.IsSubtask)
            {
                SetParent(jiraIssue);
                SetSubtasks(issue.Parent);
            }
            if (issue.Subtasks != null)
                SetSubtasks(issue);

            if (issue.Priority == null)
                _context.IssuePriorityEnabled = false;
        }

        private void SetGenericIssue(IssueDetailed issue, JiraIssue jiraIssue)
        {
            this._currentIssue = issue;
            this._currentJiraIssue = jiraIssue;

            SetEmptyEntries();
            SetPriority();
            SetStatus();
            SetAssignee();
            SetEstimatesAndRemaining();
            SetResolution();
            SetIssueType();
            SetSubtasks();
            SetLabel();
            SetDates();
            SetTimeSpent();
            IssueAdapter.SetTimeFormat(issue);
            AdjustIssuePullRequests();
            SetIssueLink();
            SetStatusType();
            SetDisplayStatus();
            SetHasWorkLoggedByAssignee();
            SetIssueExceededEstimate();
            SetIsNewProperty();
        }

        private void SetIsNewProperty()
        {
            if (_currentIssue.Created < DateTime.Now.ToOriginalTimeZone(_context.OffsetFromUtc).AddDays(-1))
                return;
            _currentIssue.IsNew = true;
        }

        private void SetSubtasks()
        {
            if (_currentJiraIssue.Fields.Subtasks != null)
                _currentIssue.Subtasks = _currentJiraIssue.Fields.Subtasks;
        }

        private void SetPriority()
        {
            _currentIssue.Priority = _currentJiraIssue.Fields.Priority;
        }

        private void SetEmptyEntries()
        {
            if (_currentIssue.Entries == null)
                _currentIssue.Entries = new List<Entry>();
        }

        private void SetIssueType()
        {
            _currentIssue.Type = _currentJiraIssue.Fields.IssueType.Name;
            _currentIssue.IsSubtask = _currentJiraIssue.Fields.IssueType.Subtask;
        }

        private void SetAssignee()
        {
            if (_currentJiraIssue.Fields.Assignee != null)
                _currentIssue.Assignee = AuthorHelpers.GetCleanName(_currentJiraIssue.Fields.Assignee.DisplayName);
        }

        private void SetStatus()
        {
            _currentIssue.Status = _currentJiraIssue.Fields.Status.Name;
            _currentIssue.PolicyReopenedStatus = _policy.AdvancedOptions.ReopenedStatus;
            _currentIssue.StatusCategory = _currentJiraIssue.Fields.Status.StatusCategory;
        }

        private void SetTimeSpent()
        {
            IssueAdapter.TimeSpentFromEntries(_currentIssue);
            _currentIssue.TimeSpentOnTask = _currentJiraIssue.Fields.TimeSpent;
            _currentIssue.TimeSpentTotal = _currentJiraIssue.Fields.AggregateTimeSpent;
        }

        private void SetDates()
        {
            _currentIssue.Created = Convert.ToDateTime(_currentJiraIssue.Fields.Created);
            _currentIssue.Updated = _currentJiraIssue.Fields.Updated;
            _currentIssue.UpdatedDate = Convert.ToDateTime(_currentIssue.Updated);
        }

        private void SetResolution()
        {
            if (_currentJiraIssue.Fields.Resolution != null)
            {
                _currentIssue.Resolution = _currentJiraIssue.Fields.Resolution.Name;
                _currentIssue.StringResolutionDate = _currentJiraIssue.Fields.ResolutionDate;
                _currentIssue.ResolutionDate = Convert.ToDateTime(_currentIssue.StringResolutionDate);
                _currentIssue.CompletedTimeAgo = TimeFormatting.GetStringDay(_currentIssue.ResolutionDate.ToOriginalTimeZone(_context.OffsetFromUtc), _context.ReportDate);
            }
        }

        private void SetEstimatesAndRemaining()
        {
            _currentIssue.RemainingEstimateSeconds = _currentJiraIssue.Fields.AggregateTimeEstimate;
            _currentIssue.TotalRemainingSeconds = _currentJiraIssue.Fields.AggregateTimeEstimate;
            _currentIssue.OriginalEstimateSecondsTotal = _currentJiraIssue.Fields.AggregaTetimeOriginalEstimate;
            _currentIssue.OriginalEstimateSeconds = _currentJiraIssue.Fields.TimeOriginalEstimate;
        }

        private void SetParent(JiraIssue jiraIssue)
        {
            _currentIssue.Parent = new IssueDetailed { Key = jiraIssue.Fields.Parent.Key, Summary = jiraIssue.Fields.Parent.Fields.Summary };
            var parent = JiraService.GetIssue(_context.JiraRequestContext, _currentIssue.Parent.Key);
            SetGenericIssue(_currentIssue.Parent, parent);
        }

        private void SetSubtasks(IssueDetailed issue)
        {
            var jiraIssue = new JiraIssue();
            issue.SubtasksDetailed = new List<IssueDetailed>();
            foreach (var task in issue.Subtasks)
            {
                jiraIssue = JiraService.GetIssue(_context.JiraRequestContext, task.Key);
                var subtask = new IssueDetailed(jiraIssue);
                issue.SubtasksDetailed.Add(subtask);
                IssueAdapter.GetEntriesFromJiraWorklogs(jiraIssue, subtask);
                SetGenericIssue(subtask, jiraIssue);
            }
        }

        private void SetLabel()
        {
            if (_currentJiraIssue.Fields.Labels == null)
                return;

            foreach (var label in _currentJiraIssue.Fields.Labels)
                if (label == _policy.AdvancedOptions.PermanentTaskLabel)
                    _currentIssue.Label = label;
        }

        private void AdjustIssuePullRequests()
        {
            if (_pullRequests != null)
            {
                _currentIssue.PullRequests = new List<JiraPullRequest>();
                _currentIssue.PullRequests = _pullRequests.FindAll(pr => IssueAdapter.ContainsKey(pr.GithubPullRequest.Title, _currentIssue.Key) == true);
                EditIssuePullRequests(_currentIssue);
            }
        }

        private void EditIssuePullRequests(IssueDetailed issue)
        {
            if (issue.PullRequests != null)
                foreach (var pullRequest in issue.PullRequests)
                    pullRequest.TaskSynced = true;
        }

        private void SetIssueLink()
        {
            Uri baseLink = new Uri(_policy.BaseUrl);
            baseLink = new Uri(baseLink, "browse/");
            _currentIssue.Link = new Uri(baseLink, _currentIssue.Key);
        }

        private void SetDisplayStatus()
        {
            if (_policy.AdvancedOptions.AdditionalWorkflowStatuses != null)
                if (_policy.AdvancedOptions.AdditionalWorkflowStatuses.Exists(s => s.ToLower() == _currentIssue.Status.ToLower()))
                    _currentIssue.DisplayStatus = true;
        }

        private void SetStatusType()
        {
            if (_currentIssue.StatusCategory.Name == "In Progress")
                _currentIssue.StatusType = "In Progress";
            else
                if (_currentIssue.Resolution == null)
                    _currentIssue.StatusType = "Open";
        }

        private void SetHasWorkLoggedByAssignee()
        {
            if (_currentIssue != null)
                if (_currentIssue.Assignee != null)
                    if (_currentIssue.ExistsInTimesheet == true)
                        _currentIssue.HasWorkLoggedByAssignee = _currentIssue.Entries.Exists(e => e.AuthorFullName == _currentIssue.Assignee);
        }

        private void SetIssueExceededEstimate()
        {
            if (_currentIssue.TimeSpentTotal <= _currentIssue.OriginalEstimateSecondsTotal || _currentIssue.Label == null)
                return;

            var percentage = MathHelpers.GetPercentage((_currentIssue.TimeSpentTotal - _currentIssue.OriginalEstimateSecondsTotal), _currentIssue.OriginalEstimateSecondsTotal);
            if (percentage >= 25)
                _currentIssue.ExceededOriginalEstimate = true;
        }
    }
}
