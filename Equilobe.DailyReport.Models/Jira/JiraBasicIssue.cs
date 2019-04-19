using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraBasicIssue : JiraIdentifiableResponse
    {
        [DataMember(Name = "expand")]
        public string Expand { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "colorName")]
        public string ColorName { get; set; }
    }
}
