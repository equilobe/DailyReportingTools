using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Comment : JiraDateIdentifiedResponse
    {
        [DataMember(Name = "author")]
        public JiraAuthorSummary Author { get; set; }

        [DataMember(Name = "body")]
        public string Body { get; set; }

        [DataMember(Name = "updateAuthor")]
        public JiraAuthorSummary UpdateAuthor { get; set; }
    }
}
