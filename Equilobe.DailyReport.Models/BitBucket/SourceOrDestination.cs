using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class SourceOrDestination
    {
        [DataMember(Name = "commit")]
        public Commit Commit { get; set; }

        [DataMember(Name = "repository")]
        public Repository Repository { get; set; }

        [DataMember(Name = "branch")]
        public Branch Branch { get; set; }
    }
}
