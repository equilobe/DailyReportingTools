using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Project
    {
        [DataMember(Name = "expand")]
        public string Expand { get; set; }

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "lead")]
        public JiraUser Lead { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "assigneeType")]
        public string AssigneeType { get; set; }
    }
}
