using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class Commit
    {
        [DataMember(Name = "hash")]
        public string Hash { get; set; }

        [DataMember(Name = "link")]
        public List<Link> Link { get; set; }
    }
}
