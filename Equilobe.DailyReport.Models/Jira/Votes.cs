using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Votes : JiraIdentifiableResponse
    {
        [DataMember(Name = "votes")]
        public int VotesCount { get; set; }

        [DataMember(Name = "hasVoted")]
        public bool HasVoted { get; set; }
    }
}
