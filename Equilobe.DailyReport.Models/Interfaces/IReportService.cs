namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportService : IService
    {
        void UpdateDashboardData(long instanceId);
        SimpleResult SyncDashboardData(string instanceUniqueKey);
    }
}
