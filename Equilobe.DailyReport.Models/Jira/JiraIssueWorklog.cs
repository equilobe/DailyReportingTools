using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraIssueWorklog
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "fields")]
        public JiraFields Fields { get; set; }
    }
}
