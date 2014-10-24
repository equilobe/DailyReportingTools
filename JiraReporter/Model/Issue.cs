using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AnotherJiraRestClient;
using AnotherJiraRestClient.JiraModel;

namespace JiraReporter.Model
{
    [Serializable]
    public class Issue
    {
        [XmlElement("key")]
        public string Key {get; set;}

        [XmlIgnore]
        public Uri Link { get; set; }
        [XmlIgnore]
        public string TimeLogged { get; set; }
        [XmlIgnore]
        public string TimeLoggedTotal { get; set; }
        [XmlIgnore]
        public int TimeSpent { get; set; }
        [XmlIgnore]
        public int TimeSpentTotal { get; set; }
        [XmlIgnore]
        public int TimeSpentOnTask { get; set; }
        [XmlIgnore]
        public string Resolution { get; set; }
        [XmlIgnore]
        public string Status { get; set; }
        [XmlIgnore]
        public string Assignee { get; set; }
        [XmlIgnore]
        public string LoggedAuthor { get; set; }
        [XmlIgnore]
        public AnotherJiraRestClient.Priority Priority { get; set; }
        [XmlIgnore]
        public int RemainingEstimateSeconds { get; set; }
        [XmlIgnore]
        public string RemainingEstimate { get; set; }
        [XmlIgnore]
        public int TotalRemainingSeconds { get; set; }
        [XmlIgnore]
        public string TotalRemaining { get; set; }
        [XmlIgnore]
        public int OriginalEstimateSeconds { get; set; }
        [XmlIgnore]
        public int OriginalEstimateSecondsTotal { get; set; }
        [XmlIgnore]
        public string Type { get; set; }
        [XmlIgnore]
        public bool SubTask { get; set; }
        [XmlIgnore]
        public Issue Parent { get; set; }
        [XmlIgnore]
        public string Label { get; set; }
        [XmlIgnore]
        public string StringResolutionDate { get; set; }
        [XmlIgnore]
        public DateTime ResolutionDate { get; set; }
        [XmlIgnore]
        public StatusCategory StatusCategory { get; set; }
        [XmlIgnore]
        public string Updated { get; set; }
        [XmlIgnore]
        public DateTime UpdatedDate { get; set; }
        [XmlIgnore]
        public List<AnotherJiraRestClient.Subtask> Subtasks { get; set; }
        [XmlIgnore]
        public List<Issue> SubtasksIssues { get; set; }
        [XmlIgnore]
        public bool ExistsInTimesheet { get; set; }
        [XmlIgnore]
        public DateTime Created { get; set; }
        [XmlIgnore]
        public List<Commit> Commits { get; set; }
        [XmlIgnore]
        public List<PullRequest> PullRequests { get; set; }
        [XmlIgnore]
        public string PolicyReopenedStatus { get; set; }

        [XmlIgnore]
        public string CompletedTimeAgo { get; set; }
        [XmlIgnore]
        public bool HasSubtasksInProgress { get; set; }
        [XmlIgnore]
        public bool HasAssignedSubtasksInProgress { get; set; }
        [XmlIgnore]
        public bool HasWorkLoggedByAssignee { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("entries", Type = typeof(Entries))]
        public List<Entries> Entries { get; set; }

        public Issue()
        {

        }

        public Issue(Issue issue)
        {
            if(issue.Assignee!=null)
                this.Assignee = issue.Assignee;
            if(issue.Entries!=null)
                this.Entries = issue.Entries;
            this.Key = issue.Key;
            if(issue.Label!=null)
                this.Label = issue.Label;
            this.Link = issue.Link;
            if (issue.Parent!=null)
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
            this.SubTask = issue.SubTask;
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
                this.SubtasksIssues = issue.SubtasksIssues;
            }
            this.Commits = issue.Commits;
            this.PullRequests = issue.PullRequests;
            this.LoggedAuthor = issue.LoggedAuthor;
            this.PolicyReopenedStatus = issue.PolicyReopenedStatus;

            this.HasAssignedSubtasksInProgress = issue.HasAssignedSubtasksInProgress;
            this.HasSubtasksInProgress = issue.HasSubtasksInProgress;
            this.CompletedTimeAgo = issue.CompletedTimeAgo;
            this.HasWorkLoggedByAssignee = issue.HasWorkLoggedByAssignee;

        }       
    }
}
