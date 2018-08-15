using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class ProjectInfo
    {
        [DataMember(Name = "id")]
        public long ProjectId { get; set; }

        [DataMember(Name = "name")]
        public string ProjectName { get; set; }

        [DataMember(Name = "key")]
        public string ProjectKey { get; set; }
    }
}
