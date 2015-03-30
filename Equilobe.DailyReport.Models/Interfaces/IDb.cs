using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IInstance
    {
        long Id { get; set; }
        string BaseUrl { get; set; }
        string TimeZone { get; set; }
    }

    public interface IBasicSettings
    {
        string BaseUrl { get; set; }
        long ProjectId { get; set; }
        string ReportTime { get; set; }
    }

    public interface IAdvancedSettings
    {
        string DraftEmails { get; set; }
        string Emails { get; set; }
        int AllocatedHoursPerDay { get; set; }
        int AllocatedHoursPerMonth { get; set; }
        SourceControlOptions SourceControlOptions { get; set;}
        List<User> UserOptions { get; set; }
        List<Month> MonthlyOptions { get; set; }
        AdvancedOptions AdvancedOptions { get; set; }
    }
}
