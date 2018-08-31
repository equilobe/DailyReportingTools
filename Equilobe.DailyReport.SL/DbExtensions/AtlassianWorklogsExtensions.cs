using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL.DbExtensions
{
    public static class AtlassianWorklogsExtensions
    {
        public static IQueryable<AtlassianWorklog> GetAtlassianWorklogs(this ReportsDb db, long instanceId)
        {
            return db.AtlassianWorklogs
                .Where(p => p.InstalledInstanceId == instanceId);
        }

        public static List<AtlassianWorklog> GetAtlassianWorklogsByJiraIds(this ReportsDb db, long instanceId, List<long> ids)
        {
            return db.AtlassianWorklogs
                .Where(p => p.InstalledInstanceId == instanceId)
                .Where(p => ids.Contains(p.JiraWorklogId))
                .ToList();
        }

        public static Dictionary<long, List<DashboardWorklog>> GetLastWorklogsByUsers(this ReportsDb db, List<long> usersIds, DateTime from, TimeSpan OffsetFromUtc, Func<AtlassianWorklog, string, DashboardWorklog> func, string baseUrl)
        {
            return db.AtlassianWorklogs
                .Where(p => usersIds.Contains(p.AtlassianUserId))
                .Where(p => p.StartedAt >= from)
                .ToList()
                .Where(p => p.StartedAt.ToOriginalTimeZone(OffsetFromUtc) >= from)
                .OrderByDescending(p => p.StartedAt)
                .GroupBy(p => p.AtlassianUserId)
                .ToDictionary(p => p.Key, p => p.Select(q => func(q, baseUrl)).ToList());
        }

        public static Dictionary<long, List<AtlassianWorklog>> GetUsersWorklogs(this ReportsDb db, List<long> usersIds)
        {
            return db.AtlassianWorklogs
                .Where(p => usersIds.Contains(p.AtlassianUserId))
                .GroupBy(p => p.AtlassianUserId)
                .ToDictionary(p => p.Key, p => p.ToList());
        }
    }
}
