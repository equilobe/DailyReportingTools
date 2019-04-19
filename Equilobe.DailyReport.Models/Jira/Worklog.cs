using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Worklog : JiraDateIdentifiedResponse
    {
        [DataMember(Name = "author")]
        public JiraAuthorSummary Author { get; set; }

        [DataMember(Name = "updateAuthor")]
        public JiraAuthorSummary UpdateAuthor { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "timeSpent")]
        public string TimeSpent { get; set; }

        [DataMember(Name = "timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }

        [DataMember(Name = "issueId")]
        public long IssueId { get; set; }
    }
}
