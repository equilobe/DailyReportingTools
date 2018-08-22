using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Status : JiraIdentifiableResponse
    {
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "iconUrl")]
        public string IconUrl { get; set; }

        [DataMember(Name = "statusColor")]
        public string StatusColor { get; set; }

        [DataMember(Name = "statusCategory")]
        public Status StatusCategory { get; set; }
    }
}
