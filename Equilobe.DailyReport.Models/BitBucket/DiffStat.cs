using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class DiffStat
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "lines_added")]
        public int LinesAdded { get; set; }

        [DataMember(Name = "lines_removes")]
        public int LinesRemoved { get; set; }

        [DataMember(Name = "old")]
        public CommitInfo Old { get; set; }

        [DataMember(Name = "new")]
        public CommitInfo New { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
