using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class CommitInfo
    {
        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "links")]
        public Links Links { get; set; }
    }
}
