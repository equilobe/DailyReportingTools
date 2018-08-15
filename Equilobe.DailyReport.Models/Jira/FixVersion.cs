using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class FixVersion : JiraIdentifiableResponse
    {
        [DataMember(Name = "archived")]
        public string Archived { get; set; }

        [DataMember(Name = "released")]
        public string Released { get; set; }

        [DataMember(Name = "releaseDate")]
        public DateTime ReleaseDate { get; set; }
    }
}
