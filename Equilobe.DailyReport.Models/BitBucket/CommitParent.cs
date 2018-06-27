using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class CommitParent
    {
        [DataMember(Name = "hash")]
        public string Hash { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }
    }
}
