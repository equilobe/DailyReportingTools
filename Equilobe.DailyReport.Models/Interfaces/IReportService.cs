using Equilobe.DailyReport.Models.Dashboard;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportService : IService
    {
        Page<DashboardItem> GetDashboardData(InstanceFilter filter);
        void UpdateDashboardData(long instanceId);
    }
}
