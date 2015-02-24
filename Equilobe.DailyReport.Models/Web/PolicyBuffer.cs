using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;

namespace Equilobe.DailyReport.Models.Web
{
    public class PolicyBuffer : IPolicy
    {
        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public string UniqueProjectKey { get; set; }
        public string ReportTime { get; set; }
        public string DraftEmails { get; set; }
        public string Emails { get; set; }
        public int AllocatedHoursPerMonth { get; set; }
        public int AllocatedHoursPerDay { get; set; }
        public SourceControlOptions SourceControlOptions { get; set; }
        public GeneratedProperties GeneratedProperties { get; set; }
        public AdvancedOptions AdvancedOptions { get; set; }
    }
}
