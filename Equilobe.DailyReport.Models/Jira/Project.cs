using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Project : JiraIdentifiableResponse
    {
        [DataMember(Name = "expand")]
        public string Expand { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "lead")]
        public JiraUser Lead { get; set; }

        [DataMember(Name = "assigneeType")]
        public string AssigneeType { get; set; }
    }
}
