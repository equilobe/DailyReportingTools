using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Watches : JiraIdentifiableResponse
    {
        [DataMember(Name = "watchCount")]
        public int WatchCount { get; set; }

        [DataMember(Name = "isWatching")]
        public bool IsWatching { get; set; }
    }
}
