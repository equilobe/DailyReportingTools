using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class CommitPage
    {
        [DataMember(Name = "pagelen")]
        public int PageLength { get; set; }

        [DataMember(Name = "values")]
        public List<Commit> Values { get; set; }

        [DataMember(Name = "next")]
        public string Next { get; set; }
    }
}
