using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class AtlassianWorklogDataService : IAtlassianWorklogDataService
    {
        #region IAtlassianWorklogDataService implementation
        public List<AtlassianUserWorklogs> GetAtlassianWorklogsByUserIds(long instanceId, List<long> usersIds)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianWorklogs
                    .Where(p => usersIds.Contains(p.AtlassianUserId))
                    .GroupBy(p => p.AtlassianUserId)
                    .AsEnumerable()
                    .Select(p => new AtlassianUserWorklogs
                    {
                        AtlassianUserId = p.Key,
                        Worklogs = p.ToList()
                    })
                    .ToList();
            }
        }

        public Dictionary<long, List<DashboardWorklog>> GetLastWorklogsByUsers(List<long> usersIds, ReportContext context, string baseUrl)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianWorklogs
                    .Where(p => usersIds.Contains(p.AtlassianUserId))
                    .Where(p => p.StartedAt >= context.BusinessDaysAgo)
                    .ToList()
                    .Where(p => p.StartedAt.ToOriginalTimeZone(context.OffsetFromUtc) >= context.BusinessDaysAgo)
                    .OrderByDescending(p => p.StartedAt)
                    .GroupBy(p => p.AtlassianUserId)
                    .ToDictionary(p => p.Key, p => p.Select(q => ToDashboardWorklog(q, baseUrl)).ToList());
            }
        }

        public void SyncAtlassianWroklogs(List<AtlassianWorklog> jiraWorklogs, List<long> deletedWorklogsIds, long instanceId)
        {
            UpdateDeletedWorklogs(deletedWorklogsIds, instanceId);
            SyncUpdatedWorklogs(jiraWorklogs, instanceId);
        }
        #endregion

        #region Update methods
        private void UpdateDeletedWorklogs(List<long> deletedWorklogsIds, long instanceId)
        {
            if (deletedWorklogsIds == null || !deletedWorklogsIds.Any())
                return;

            using (var db = new ReportsDb())
            {
                var dbWorklogs = GetAtlassianWorklogsByJiraIds(db, instanceId, deletedWorklogsIds);

                db.AtlassianWorklogs.RemoveRange(dbWorklogs);

                db.SaveChanges();
            }
        }

        private void SyncUpdatedWorklogs(List<AtlassianWorklog> jiraWorklogs, long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var dbWorklogs = db.AtlassianWorklogs
                    .Where(p => p.InstalledInstanceId == instanceId);

                foreach (var worklog in jiraWorklogs)
                {
                    var dbWorklog = dbWorklogs.SingleOrDefault(p => p.JiraWorklogId == worklog.JiraWorklogId);

                    if (dbWorklog == null)
                        db.AtlassianWorklogs.Add(worklog);
                    else if (dbWorklog.UpdatedAt != worklog.UpdatedAt)
                        UpdateDbWorklog(dbWorklog, worklog);
                }

                db.SaveChanges();
            }
        }
        #endregion

        #region Helpers
        private List<AtlassianWorklog> GetAtlassianWorklogsByJiraIds(ReportsDb db, long instanceId, List<long> ids)
        {
            return db.AtlassianWorklogs
                .Where(p => p.InstalledInstanceId == instanceId)
                .Where(p => ids.Contains(p.JiraWorklogId))
                .ToList();
        }

        private void UpdateDbWorklog(AtlassianWorklog dbWorklog, AtlassianWorklog jiraWorklog)
        {
            dbWorklog.Comment = jiraWorklog.Comment;
            dbWorklog.UpdatedAt = jiraWorklog.UpdatedAt;
            dbWorklog.StartedAt = jiraWorklog.StartedAt;
            dbWorklog.TimeSpentInSeconds = jiraWorklog.TimeSpentInSeconds;
        }

        private DashboardWorklog ToDashboardWorklog(AtlassianWorklog worklog, string baseUrl)
        {
            return new DashboardWorklog
            {
                Comment = worklog.Comment,
                IssueKey = worklog.IssueKey,
                IssueUrl = baseUrl + "browse/" + worklog.IssueKey,
                TimeSpentInSeconds = worklog.TimeSpentInSeconds,
                Date = worklog.StartedAt
            };
        }
        #endregion
    }
}
