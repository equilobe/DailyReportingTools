using Equilobe.DailyReport.Models.ReportFrame;
using System;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class ReportContext
    {
        public long InstanceId { get; set; }
        public TimeSpan OffsetFromUtc { get; set; }
        public DateTime BusinessDaysAgo { get; set; }
        public JiraRequestContext JiraRequestContext { get; set; }
    }
}
