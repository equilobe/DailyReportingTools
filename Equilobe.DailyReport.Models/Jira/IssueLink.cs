using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class IssueLink : JiraIdentifiableResponse
    {
        [DataMember(Name = "type")]
        public IssueLink Type { get; set; }

        [DataMember(Name = "outwardIssue")]
        public JiraIssue OutwardIssue { get; set; }

        [DataMember(Name = "inwardIssue")]
        public JiraIssue InwardIssue { get; set; }
    }
}
