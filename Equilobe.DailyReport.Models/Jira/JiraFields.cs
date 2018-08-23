using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraFields : JiraDateIdentifiedResponse
    {
        [DataMember(Name = "progress")]
        public Progress Progress { get; set; }

        [DataMember(Name = "summary")]
        public string Summary { get; set; }

        [DataMember(Name = "timetracking")]
        public TimeTracking TimeTracking { get; set; }

        [DataMember(Name = "issuetype")]
        public IssueType IssueType { get; set; }

        [DataMember(Name = "votes")]
        public Votes Votes { get; set; }

        [DataMember(Name = "resolution")]
        public JiraBasicIssue Resolution { get; set; }

        [DataMember(Name = "fixVersions")]
        public List<FixVersion> FixVersions { get; set; }

        [DataMember(Name = "resolutiondate")]
        public string ResolutionDate { get; set; }

        [DataMember(Name = "timespent")]
        public int TimeSpent { get; set; }

        [DataMember(Name = "reporter")]
        public JiraAuthorSummary Reporter { get; set; }

        [DataMember(Name = "aggregatetimeoriginalestimate")]
        public int AggregaTetimeOriginalEstimate { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "priority")]
        public Status Priority { get; set; }

        [DataMember(Name = "duedate")]
        public string DueDate { get; set; }

        [DataMember(Name = "issuelinks")]
        public List<IssueLink> IssueLinks { get; set; }

        [DataMember(Name = "watches")]
        public Watches Watches { get; set; }

        [DataMember(Name = "worklog")]
        public JiraResponse<Worklog> Worklog { get; set; }

        [DataMember(Name = "subtasks")]
        public List<JiraIssue> Subtasks { get; set; }

        [DataMember(Name = "status")]
        public Status Status { get; set; }

        [DataMember(Name = "labels")]
        public List<string> Labels { get; set; }

        [DataMember(Name = "workratio")]
        public long WorkRatio { get; set; }

        [DataMember(Name = "assignee")]
        public JiraAuthorSummary Assignee { get; set; }

        [DataMember(Name = "attachment")]
        public List<object> Attachment { get; set; }

        [DataMember(Name = "aggregatetimeestimate")]
        public int AggregateTimeEstimate { get; set; }

        [DataMember(Name = "project")]
        public JiraBasicIssue Project { get; set; }

        [DataMember(Name = "parent")]
        public JiraIssue Parent { get; set; }

        [DataMember(Name = "versions")]
        public List<object> Versions { get; set; }

        [DataMember(Name = "environment")]
        public string Environment { get; set; }

        [DataMember(Name = "timeestimate")]
        public int TimeEstimate { get; set; }

        [DataMember(Name = "aggregateprogress")]
        public Progress AggregateProgress { get; set; }

        [DataMember(Name = "components")]
        public List<JiraBasicIssue> Components { get; set; }

        [DataMember(Name = "comment")]
        public JiraResponse<Comment> Comment { get; set; }

        [DataMember(Name = "timeoriginalestimate")]
        public int TimeOriginalEstimate { get; set; }

        [DataMember(Name = "aggregatetimespent")]
        public int AggregateTimeSpent { get; set; }
    }
}
