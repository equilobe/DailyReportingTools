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

namespace JiraReporter
{
    class IssueProcessor
    {
        JiraPolicy _policy
        {
            get
            {
                return _context.Policy;
            }
        }
        List<JiraPullRequest> _pullRequests;
        CompleteIssue _currentIssue;
        JiraIssue _currentJiraIssue;
        JiraReport _context;

        public IssueProcessor(JiraReport context)
        {
            this._context = context;
            this._pullRequests = context.PullRequests;
        }

        public void SetIssues(List<CompleteIssue> issues)
        {
            foreach (var issue in issues)
            {
                var jiraIssue = new JiraIssue();
                jiraIssue = new JiraService().GetIssue(_context.JiraRequestContext, issue.Key);
                SetIssue(issue, jiraIssue);
            }
        }

        public void SetIssue(CompleteIssue issue, JiraIssue jiraIssue)
        {
            SetGenericIssue(issue, jiraIssue);
            if (issue.IsSubtask)
            {
                SetParent(jiraIssue);
                SetSubtasks(issue.Parent);
            }
            if (issue.Subtasks != null)
                SetSubtasks(issue);
        }

        private void SetGenericIssue(CompleteIssue issue, JiraIssue jiraIssue)
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
            if (_currentJiraIssue.fields.subtasks != null)
                _currentIssue.Subtasks = _currentJiraIssue.fields.subtasks;
        }

        private void SetPriority()
        {
            _currentIssue.Priority = _currentJiraIssue.fields.priority;
        }

        private void SetEmptyEntries()
        {
            if (_currentIssue.Entries == null)
                _currentIssue.Entries = new List<Entry>();
        }

        private void SetIssueType()
        {
            _currentIssue.Type = _currentJiraIssue.fields.issuetype.name;
            _currentIssue.IsSubtask = _currentJiraIssue.fields.issuetype.subtask;
        }

        private void SetAssignee()
        {
            if (_currentJiraIssue.fields.assignee != null)
                _currentIssue.Assignee = AuthorHelpers.GetCleanName(_currentJiraIssue.fields.assignee.displayName);
        }

        private void SetStatus()
        {
            _currentIssue.Status = _currentJiraIssue.fields.status.name;
            _currentIssue.PolicyReopenedStatus = _policy.AdvancedOptions.ReopenedStatus;
            _currentIssue.StatusCategory = _currentJiraIssue.fields.status.statusCategory;
        }

        private void SetTimeSpent()
        {
            IssueAdapter.TimeSpentFromEntries(_currentIssue);
            _currentIssue.TimeSpentOnTask = _currentJiraIssue.fields.timespent;
            _currentIssue.TimeSpentTotal = _currentJiraIssue.fields.aggregatetimespent;
        }

        private void SetDates()
        {
            _currentIssue.Created = Convert.ToDateTime(_currentJiraIssue.fields.created);
            _currentIssue.Updated = _currentJiraIssue.fields.updated;
            _currentIssue.UpdatedDate = Convert.ToDateTime(_currentIssue.Updated);
        }

        private void SetResolution()
        {
            if (_currentJiraIssue.fields.resolution != null)
            {
                _currentIssue.Resolution = _currentJiraIssue.fields.resolution.name;
                _currentIssue.StringResolutionDate = _currentJiraIssue.fields.resolutiondate;
                _currentIssue.ResolutionDate = Convert.ToDateTime(_currentIssue.StringResolutionDate);
                _currentIssue.CompletedTimeAgo = TimeFormatting.GetStringDay(_currentIssue.ResolutionDate.ToOriginalTimeZone(_context.OffsetFromUtc), _context.ReportDate);
            }
        }

        private void SetEstimatesAndRemaining()
        {
            _currentIssue.RemainingEstimateSeconds = _currentJiraIssue.fields.aggregatetimeestimate;
            _currentIssue.TotalRemainingSeconds = _currentJiraIssue.fields.aggregatetimeestimate;
            _currentIssue.OriginalEstimateSecondsTotal = _currentJiraIssue.fields.aggregatetimeoriginalestimate;
            _currentIssue.OriginalEstimateSeconds = _currentJiraIssue.fields.timeoriginalestimate;
        }

        private void SetParent(JiraIssue jiraIssue)
        {
            _currentIssue.Parent = new CompleteIssue { Key = jiraIssue.fields.parent.key, Summary = jiraIssue.fields.parent.fields.summary };
            var parent = new JiraService().GetIssue(_context.JiraRequestContext, _currentIssue.Parent.Key);
            SetGenericIssue(_currentIssue.Parent, parent);
        }

        private void SetSubtasks(CompleteIssue issue)
        {
            var jiraIssue = new JiraIssue();
            issue.SubtasksIssues = new List<CompleteIssue>();
            foreach (var task in issue.Subtasks)
            {
                jiraIssue = new JiraService().GetIssue(_context.JiraRequestContext, task.key);
                issue.SubtasksIssues.Add(new CompleteIssue(jiraIssue));
                SetGenericIssue(issue.SubtasksIssues.Last(), jiraIssue);
            }
        }

        private void SetLabel()
        {
            foreach (var label in _currentJiraIssue.fields.labels)
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

        private void EditIssuePullRequests(CompleteIssue issue)
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
                if (_policy.AdvancedOptions.AdditionalWorkflowStatuses.Exists(s => s == _currentIssue.Status))
                    _currentIssue.DisplayStatus = true;
        }

        private void SetStatusType()
        {
            if (_currentIssue.StatusCategory.name == "In Progress")
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
