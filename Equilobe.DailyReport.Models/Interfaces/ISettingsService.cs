using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISettingsService : IService
    {
        BasicReportSettings GetBasicSettings(ItemContext context);
        List<BasicReportSettings> GetAllBasicSettings(ItemContext context);
        void SetAllBasicSettings(ItemContext context);

        AdvancedReportSettings GetAdvancedSettings(ItemContext context);

        FullReportSettings GetFullSettings(ItemContext context);
        FullReportSettings GetSyncedSettings(ItemContext context);
    }
}
