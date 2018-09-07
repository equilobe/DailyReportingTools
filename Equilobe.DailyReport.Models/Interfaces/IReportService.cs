using Equilobe.DailyReport.Models.Dashboard;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportService : IService
    {
        List<DashboardItem> GetDashboardData(long instanceId);
        void UpdateDashboardData(long instanceId);
        SimpleResult SyncDashboardData(string instanceUniqueKey);
    }
}
