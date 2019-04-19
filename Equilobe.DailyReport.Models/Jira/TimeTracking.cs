using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class TimeTracking
    {
        [DataMember(Name = "originalEstimate")]
        public string OriginalEstimate { get; set; }

        [DataMember(Name = "remainingEstimate")]
        public string RemainingEstimate { get; set; }

        [DataMember(Name = "timeSpent")]
        public string TimeSpent { get; set; }

        [DataMember(Name = "originalEstimateSeconds")]
        public int OriginalEstimateSeconds { get; set; }

        [DataMember(Name = "remainingEstimateSeconds")]
        public int RemainingEstimateSeconds { get; set; }

        [DataMember(Name = "timeSpentSeconds")]
        public int TimeSpentSeconds { get; set; }
    }
}
