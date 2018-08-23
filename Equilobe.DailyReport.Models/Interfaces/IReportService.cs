namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportService : IService
    {
        void UpdateDashboardData(long instanceId);
        SimpleResult TrySyncJiraDB(string instanceUniqueKey);
    }
}
