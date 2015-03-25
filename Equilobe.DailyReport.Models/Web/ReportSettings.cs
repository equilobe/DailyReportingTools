using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Web
{
    public class Instance : IInstance
    {
        public long Id { get; set; }
        public string BaseUrl { get; set; }
    }

    public class BasicReportSettings : IBasicSettings
    {
        public long Id { get; set; }
        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }
        public string ProjectKey { get; set; }
        public string UniqueProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string ReportTime { get; set; }
    }

    public class AdvancedReportSettings : IAdvancedSettings
    {
        public string DraftEmails { get; set; }
        public string Emails { get; set; }
        public int AllocatedHoursPerDay { get; set; }
        public int AllocatedHoursPerMonth { get; set; }
        public SourceControlOptions SourceControlOptions { get; set; }
        public List<User> UserOptions { get; set; }
        public List<Month> MonthlyOptions { get; set; }
        public AdvancedOptions AdvancedOptions { get; set; }
    }

    public class FullReportSettings : IBasicSettings, IAdvancedSettings
    {
        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public string UniqueProjectKey { get; set; }
        public string ReportTime { get; set; }
        public string DraftEmails { get; set; }
        public string Emails { get; set; }
        public int AllocatedHoursPerDay { get; set; }
        public int AllocatedHoursPerMonth { get; set; }
        public SourceControlOptions SourceControlOptions { get; set; }
        public List<string> SourceControlUsernames { get; set; }
        public List<User> UserOptions { get; set; }
        public List<Month> MonthlyOptions { get; set; }
        public AdvancedOptions AdvancedOptions { get; set; }
    }
}
