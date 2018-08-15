using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraIdentifiableResponse
    {
        [DataMember(Name = "id")]
        public long Id { get; set; }

        [DataMember(Name = "key")]
        public string Key { get; set; }
        
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "self")]
        public string Self { get; set; }
    }
}
