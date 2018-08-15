using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class IssueDetailed
    {
        public string Key { get; set; }

        public Uri Link { get; set; }
        public string TimeLogged { get; set; }
        public string TimeLoggedTotal { get; set; }
        public int TimeSpent { get; set; }
        public int TimeSpentTotal { get; set; }
        public int TimeSpentOnTask { get; set; }
        public string Resolution { get; set; }
        public string Status { get; set; }
        public string Assignee { get; set; }
        public string LoggedAuthor { get; set; }
        public Priority Priority { get; set; }
        public int RemainingEstimateSeconds { get; set; }
        public string RemainingEstimate { get; set; }
        public int TotalRemainingSeconds { get; set; }
        public string TotalRemaining { get; set; }
        public int OriginalEstimateSeconds { get; set; }
        public int OriginalEstimateSecondsTotal { get; set; }
        public bool ExceededOriginalEstimate { get; set; }
        public string Type { get; set; }
        public bool IsSubtask { get; set; }
        public IssueDetailed Parent { get; set; }
        public string Label { get; set; }
        public string StringResolutionDate { get; set; }
        public DateTime ResolutionDate { get; set; }
        public Status StatusCategory { get; set; }
        public string Updated { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<JiraIssue> Subtasks { get; set; }
        public List<IssueDetailed> SubtasksDetailed { get; set; }
        public bool ExistsInTimesheet { get; set; }
        public DateTime Created { get; set; }
        public List<JiraCommit> Commits { get; set; }
        public List<JiraPullRequest> PullRequests { get; set; }
        public string PolicyReopenedStatus { get; set; }
        public int ErrorsCount { get; set; }
        public List<Error> Errors { get; set; }
        public string StatusType { get; set; }
        public bool DisplayStatus { get; set; }
        public bool IsNew { get; set; }
        public string CompletedTimeAgo { get; set; }
        public bool HasSubtasksInProgress { get; set; }
        public bool HasAssignedSubtasksInProgress { get; set; }
        public bool HasWorkLoggedByAssignee { get; set; }
        public string Summary { get; set; }
        public List<Entry> Entries { get; set; }
        public bool HasWorkLogged { get; set; }
        public bool NotFromSprint { get; set; }

        public IssueDetailed()
        {

        }

        public IssueDetailed(IssueDetailed issue)
        {
            if (issue.Assignee != null)
                this.Assignee = issue.Assignee;
            if (issue.Entries != null)
                this.Entries = issue.Entries;
            this.Key = issue.Key;
            if (issue.Label != null)
                this.Label = issue.Label;
            this.Link = issue.Link;
            if (issue.Parent != null)
                this.Parent = issue.Parent;
            this.Priority = issue.Priority;
            this.RemainingEstimate = issue.RemainingEstimate;
            this.RemainingEstimateSeconds = issue.RemainingEstimateSeconds;
            this.TotalRemainingSeconds = issue.TotalRemainingSeconds;
            this.TotalRemaining = issue.TotalRemaining;
            this.OriginalEstimateSeconds = issue.OriginalEstimateSeconds;
            this.OriginalEstimateSecondsTotal = issue.OriginalEstimateSecondsTotal;
            if (issue.Resolution != null)
            {
                this.Resolution = issue.Resolution;
                this.ResolutionDate = issue.ResolutionDate;
                this.StringResolutionDate = issue.StringResolutionDate;
            }
            this.Status = issue.Status;
            this.IsSubtask = issue.IsSubtask;
            this.Summary = issue.Summary;
            this.TimeLogged = issue.TimeLogged;
            this.TimeLoggedTotal = issue.TimeLoggedTotal;
            this.TimeSpentTotal = issue.TimeSpentTotal;
            this.TimeSpent = issue.TimeSpent;
            this.TimeSpentOnTask = issue.TimeSpentOnTask;
            this.Type = issue.Type;
            this.StatusCategory = issue.StatusCategory;
            this.Created = issue.Created;
            this.Updated = issue.Updated;
            this.UpdatedDate = issue.UpdatedDate;
            this.ExistsInTimesheet = issue.ExistsInTimesheet;
            if (issue.Subtasks != null)
            {
                this.Subtasks = issue.Subtasks;
                this.SubtasksDetailed = issue.SubtasksDetailed;
            }
            this.Commits = issue.Commits;
            this.PullRequests = issue.PullRequests;
            this.LoggedAuthor = issue.LoggedAuthor;
            this.PolicyReopenedStatus = issue.PolicyReopenedStatus;

            this.HasAssignedSubtasksInProgress = issue.HasAssignedSubtasksInProgress;
            this.HasSubtasksInProgress = issue.HasSubtasksInProgress;
            this.CompletedTimeAgo = issue.CompletedTimeAgo;
            this.HasWorkLoggedByAssignee = issue.HasWorkLoggedByAssignee;
            this.StatusType = issue.StatusType;
            this.DisplayStatus = issue.DisplayStatus;
            this.NotFromSprint = issue.NotFromSprint;
        }

        public IssueDetailed(JiraIssue jiraIssue)
        {
            Summary = jiraIssue.Fields.Summary;
            Key = jiraIssue.Key;
        }
    }
}
