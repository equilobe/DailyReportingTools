using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Progress
    {
        [DataMember(Name = "progress")]
        public int Progressed { get; set; }

        [DataMember(Name = "total")]
        public int Total { get; set; }

        [DataMember(Name = "percent")]
        public int Percent { get; set; }
    }
}
