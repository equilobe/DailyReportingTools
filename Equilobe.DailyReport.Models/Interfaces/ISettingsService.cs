﻿using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISettingsService : IService
    {
        BasicReportSettings GetBasicReportSettings(ItemContext context);
        List<List<BasicReportSettings>> GetAllBasicReportSettings(UserContext context);
        List<BasicReportSettings> GetAllBasicReportSettings(ItemContext context);
        void SyncAllBasicSettings(ItemContext context);

        AdvancedReportSettings GetAdvancedReportSettings(ItemContext context);

        FullReportSettings GetFullReportSettings(ItemContext context);
        FullReportSettings GetSyncedReportSettings(ItemContext context);
    }
}
