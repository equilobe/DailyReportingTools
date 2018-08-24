namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IConfigurationService : IService
    {
        string GetAddonKey();
        string GetWebBaseUrl();
        string GetEncriptedKey();
        string GetJiraReporterPath();
        string GetReportToolPath();
        string GetTaskSchedulerFolderName();
        string GetTimeZoneMappingPath();
        bool IsPaypalSandbox();
        string GetBitBucketApiClientUrl();
        string GetDashboardDataSyncToolPath();
    }
}
