using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.Models.Web
{
    public class PolicySummary : IPolicySummary
    {
        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string ReportTime { get; set; }
    }
}