using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicySummaryService : IService
    {
        System.Collections.Generic.List<Equilobe.DailyReport.Models.Web.PolicySummary> GetPoliciesSummary();
        Equilobe.DailyReport.Models.Web.PolicySummary GetPolicySummary(long projectId);
    }
}
