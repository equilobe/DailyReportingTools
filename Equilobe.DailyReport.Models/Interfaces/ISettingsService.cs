using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISettingsService : IService
    {
        BasicReportSettings GetBasicReportSettings(ItemContext context);

        List<JiraInstance> GetAllBasicReportSettings(UserContext context);

        List<BasicReportSettings> GetAllBasicReportSettings(ItemContext context);

        void SyncAllBasicSettings(ItemContext context);

        AdvancedReportSettings GetAdvancedReportSettings(ItemContext context);

        FullReportSettings GetFullReportSettings(ItemContext context);

        FullReportSettings GetSyncedReportSettings(ItemContext context);

        void SetFullReportSettings(FullReportSettings updatedFullSettings);

    }
}
