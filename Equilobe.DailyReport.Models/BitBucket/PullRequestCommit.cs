using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class PullRequestCommit
    {
        [DataMember(Name = "hash")]
        public string Hash { get; set; }

        [DataMember(Name = "link")]
        public Links Link { get; set; }
    }
}
