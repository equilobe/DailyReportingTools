using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.BitBucket
{
    [DataContract]
    public class BitBucketResponsePage<T>
    {
        [DataMember(Name = "pagelen")]
        public int PageLength { get; set; }

        [DataMember(Name = "size")]
        public int Size { get; set; }

        [DataMember(Name = "page")]
        public int Page { get; set; }

        [DataMember(Name = "next")]
        public string Next { get; set; }

        [DataMember(Name = "previous")]
        public string Previous { get; set; }

        [DataMember(Name = "values")]
        public List<T> Values { get; set; }
    }
}
