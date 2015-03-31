using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISettingsService : IService
    {
        BasicReportSettings GetBasicReportSettings(ItemContext context);
        List<BasicReportSettings> GetAllBasicReportSettings(ItemContext context);
        void SetAllBasicSettings(ItemContext context);

        AdvancedReportSettings GetAdvancedReportSettings(ItemContext context);

        FullReportSettings GetFullReportSettings(ItemContext context);
        FullReportSettings GetSyncedReportSettings(ItemContext context);
    }
}
