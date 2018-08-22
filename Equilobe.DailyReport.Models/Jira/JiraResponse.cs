using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraResponse<T>
    {
        [DataMember(Name = "maxResults")]
        public long MaxResults { get; set; }

        [DataMember(Name = "startedAt")]
        public long StartAt { get; set; }

        [DataMember(Name = "total")]
        public long Total { get; set; }

        [DataMember(Name = "isLast")]
        public bool IsLast { get; set; }

        [DataMember(Name = "values")]
        public List<T> Values { get; set; }

        [DataMember(Name = "issues")]
        public List<T> Issues { get; set; }
    }
}
