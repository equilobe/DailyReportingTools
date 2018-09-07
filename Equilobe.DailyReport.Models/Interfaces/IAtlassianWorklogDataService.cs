using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IAtlassianWorklogDataService : IService
    {
        List<AtlassianUserWorklogs> GetAtlassianWorklogsByUserIds(long instanceId, List<long> users);
        Dictionary<long, List<DashboardWorklog>> GetLastWorklogsByUsers(List<long> usersIds, ReportContext context, string baseUrl);
        void SyncAtlassianWroklogs(List<AtlassianWorklog> jiraWorklogs, List<long> deletedWorklogsIds, ReportContext context, DateTime lastSync);
    }
}
