using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicySummaryService : IService
    {
        List<ReportSettingsSummary> GetPoliciesSummary(ItemContext context);
        ReportSettingsSummary GetPolicySummary(ItemContext context);
    }
}
