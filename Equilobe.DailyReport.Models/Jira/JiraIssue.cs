using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraIssue : JiraBasicIssue
    {
        [DataMember(Name = "fields")]
        public JiraFields Fields { get; set; }
    }
}
