using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class JiraWorklogs
    {
        [DataMember(Name = "since")]
        public long Since { get; set; }

        [DataMember(Name = "until")]
        public long Until { get; set; }

        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "lastPage")]
        public bool IsLastPage { get; set; }

        [DataMember(Name = "values")]
        public List<WorklogSummary> Values { get; set; }
    }

    [DataContract]
    public class WorklogSummary
    {
        [DataMember(Name = "worklogId")]
        public long WorklogId { get; set; }

        [DataMember(Name = "updatedTime")]
        public long UpdatedTime { get; set; }
    }
}
