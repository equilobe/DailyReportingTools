using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Priority : Status
    {
        public static Status UNKNOWN_PRIORITY = new Status()
        {
            Name = "Unknown",
            Description = "Unknown priority",
            StatusColor = string.Empty,
            IconUrl = string.Empty,
            Self = string.Empty
        };
    }
}
