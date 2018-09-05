﻿using Equilobe.DailyReport.DAL;
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
        public Dictionary<long, List<AtlassianWorklog>> GetAtlassianWorklogsByUserIds(long instanceId, List<long> usersIds)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianWorklogs
                    .Where(p => usersIds.Contains(p.AtlassianUserId))
                    .GroupBy(p => p.AtlassianUserId)
                    .ToDictionary(p => p.Key, p => p.ToList());
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

        public void SyncAtlassianWroklogs(List<AtlassianWorklog> jiraWorklogs, List<long> deletedWorklogsIds, ReportContext context, DateTime lastSync)
        {
            UpdateDeletedWorklogs(deletedWorklogsIds, context, lastSync);
            SyncUpdatedWorklogs(jiraWorklogs, context, lastSync);
        }
        #endregion

        #region Update methods
        private void UpdateDeletedWorklogs(List<long> deletedWorklogsIds, ReportContext context, DateTime lastSync)
        {
            if (deletedWorklogsIds == null || !deletedWorklogsIds.Any())
                return;

            using (var db = new ReportsDb())
            {
                var dbWorklogs = GetAtlassianWorklogsByJiraIds(db, context.InstanceId, deletedWorklogsIds);

                db.AtlassianWorklogs.RemoveRange(dbWorklogs);

                db.SaveChanges();
            }
        }

        private void SyncUpdatedWorklogs(List<AtlassianWorklog> jiraWorklogs, ReportContext context, DateTime lastSync)
        {
            var dbWorklogs = GetWorklogs(context.InstanceId);

            using (var db = new ReportsDb())
            {
                foreach (var worklog in jiraWorklogs)
                {
                    var dbWorklog = dbWorklogs.Where(p => p.JiraWorklogId == worklog.JiraWorklogId).SingleOrDefault();

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
            dbWorklog.TimeSpentInSeconds = jiraWorklog.TimeSpentInSeconds;
        }

        private List<AtlassianWorklog> GetWorklogs(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianWorklogs
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();
            }
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
