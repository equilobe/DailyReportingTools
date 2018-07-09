using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class CommitAuthor
    {
        [DataMember(Name = "raw")]
        public string Raw { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "user")]
        public Author User { get; set; }
    }
}
