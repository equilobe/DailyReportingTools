using Equilobe.DailyReport.Models.Storage;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicy
    {
        string BaseUrl { get; set; }
        long ProjectId { get; set; }
        string ProjectName { get; set; }
        string ProjectKey { get; set; }
        string UniqueProjectKey { get; set; }
        string ReportTime { get; set; }
        string DraftEmails { get; set; }
        string Emails { get; set; }
        int AllocatedHoursPerMonth { get; set; }
        int AllocatedHoursPerDay { get; set; }
        SourceControlOptions SourceControlOptions { get; set;}
        GeneratedProperties GeneratedProperties { get; set; }
        AdvancedOptions AdvancedOptions { get; set; }
    }
}
