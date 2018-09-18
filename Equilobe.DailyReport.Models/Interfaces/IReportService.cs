using Equilobe.DailyReport.Models.Dashboard;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportService : IService
    {
        DashboardData GetDashboardData(DashboardFilter filter);
        bool IsDashboardAvailable(DashboardFilter filter);
        void UpdateDashboardData(long instanceId);
        SimpleResult SyncDashboardData(string instanceUniqueKey);
    }
}
