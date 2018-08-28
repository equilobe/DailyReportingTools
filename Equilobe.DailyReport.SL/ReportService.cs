using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class ReportService : IReportService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public JiraRequestContext JiraRequestContext { get; set; }

        #region IReportService Implementation
        public Page<DashboardItem> GetDashboardData(InstanceFilter filter)
        {
            return new Page<DashboardItem>
            {
                Items = GetDashboardUsers(filter),
                TotalRecords = GetTotalAtlassianUsers(filter.InstanceId),
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };
        }

        public SimpleResult SyncDashboardData(string instanceUniqueKey)
        {
            var instance = DataService.GetInstanceByKey(instanceUniqueKey);

            if (instance == null)
            {
                TaskSchedulerService.DeleteDashboardDataSyncTask(instanceUniqueKey);
                return SimpleResult.Error("Invalid instance unique key");
            }

            UpdateDashboardData(instance.Id);

            return SimpleResult.Success("Sync successfuly");
        }

        public void UpdateDashboardData(long instanceId)
        {
            JiraRequestContext = GetJiraRequestContext(instanceId);

            SyncAtlassianUsers(instanceId);
            SyncAtlassianWorklogs(instanceId);
            UpdateLastSyncDate(instanceId);

            CreateOrUpdateSyncScheduleTask(instanceId);
        }
        #endregion

        #region Helpers
        private List<DashboardItem> GetDashboardUsers(InstanceFilter filter)
        {
            throw new NotImplementedException();
        }

        private int GetTotalAtlassianUsers(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianUsers
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Count();
            }
        }

        private void SyncAtlassianWorklogs(long instanceId)
        {
            var lastSync = GetLastSyncDate(instanceId);

            SyncDeletedWorklogs(instanceId, lastSync);
            SyncUpdatedWorklogs(instanceId, lastSync);
        }

        private void SyncDeletedWorklogs(long instanceId, DateTime lastSync)
        {
            var deletedWorklogsIds = JiraService.GetDeletedWorklogsIds(JiraRequestContext, lastSync);

            if (deletedWorklogsIds == null)
                return;

            using (var db = new ReportsDb())
            {
                var dbWorklogs = db.AtlassianWorklogs
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Where(p => deletedWorklogsIds.Contains(p.JiraWorklogId))
                    .ToList();

                db.AtlassianWorklogs.RemoveRange(dbWorklogs);

                db.SaveChanges();
            }
        }

        private void SyncUpdatedWorklogs(long instanceId, DateTime lastSync)
        {
            var worklogs = GetAtlassianWorklogs(instanceId, lastSync);

            using (var db = new ReportsDb())
            {
                var dbWorklogs = db.AtlassianWorklogs
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();

                foreach (var worklog in worklogs)
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

        private void SyncAtlassianUsers(long instanceId)
        {
            var users = JiraService.GetAllUsers(JiraRequestContext)
                .Select(p => ToAtlassianUser(p, instanceId))
                .ToList();

            var dbUsers = GetAllAtlassianUsers(instanceId);

            using (var db = new ReportsDb())
            {
                foreach (var user in users)
                {
                    var dbUser = dbUsers.Where(p => p.Key == user.Key).SingleOrDefault();

                    if (dbUser == null)
                        db.AtlassianUsers.Add(user);
                    else
                        UpdateDbUser(dbUser, user);
                }

                db.SaveChanges();
            }
        }

        private void UpdateLastSyncDate(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var instance = db.InstalledInstances.SingleOrDefault(p => p.Id == instanceId);

                instance.LastSync = DateTime.UtcNow;

                db.SaveChanges();
            }
        }

        private void CreateOrUpdateSyncScheduleTask(long instanceId)
        {
            var instanceKey = DataService.GetInstance(instanceId).UniqueKey;

            TaskSchedulerService.CreateDashboardDataSyncTask(instanceKey);
        }

        private DateTime GetLastSyncDate(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var lastSync = db.InstalledInstances
                    .First(p => p.Id == instanceId)
                    .LastSync;

                return lastSync ?? DateTime.UtcNow.AddMonths(-1);
            }
        }

        private List<AtlassianWorklog> GetAtlassianWorklogs(long instanceId, DateTime lastSync)
        {
            var users = GetAllAtlassianUsers(instanceId);
            var userKeys = users
                .Select(p => p.Key)
                .ToList();

            var offsetFromUtc = DataService.GetOffsetFromInstanceId(instanceId);
            var fromDate = lastSync.ToOriginalTimeZone(offsetFromUtc);
            var issueWorklogs = JiraService.GetWorklogsForMultipleUsers(JiraRequestContext, userKeys, fromDate);
            var worklogs = GetWorklogsFromIssueWorklogs(issueWorklogs, users, instanceId);

            return worklogs;
        }

        private List<AtlassianUser> GetAllAtlassianUsers(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianUsers
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();
            }
        }

        private JiraRequestContext GetJiraRequestContext(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var instance = db.InstalledInstances.SingleOrDefault(p => p.Id == instanceId);
                var jiraRequestContext = new JiraRequestContext();

                if (instance == null)
                    throw new Exception("Invalid instance id");

                instance.CopyPropertiesOnObjects(jiraRequestContext);

                return jiraRequestContext;
            }
        }

        private AtlassianUser ToAtlassianUser(JiraUser user, long instanceId)
        {
            return new AtlassianUser
            {
                DisplayName = user.DisplayName,
                InstalledInstanceId = instanceId,
                Key = user.Key,
                EmailAddress = user.EmailAddress,
                Avatar16x16 = user.AvatarUrls.VerySmall.AbsoluteUri,
                Avatar24x24 = user.AvatarUrls.Small.AbsoluteUri,
                Avatar32x32 = user.AvatarUrls.Med.AbsoluteUri,
                Avatar48x48 = user.AvatarUrls.Big.AbsoluteUri,
                IsActive = user.IsActive
            };
        }

        private List<AtlassianWorklog> GetWorklogsFromIssueWorklogs(List<JiraIssue> issueWorklog, List<AtlassianUser> users, long instanceId)
        {
            var worklogs = new List<AtlassianWorklog>();

            foreach (var issue in issueWorklog)
            {
                foreach (var worklog in issue.Fields.Worklog.Worklogs)
                {
                    var user = users.SingleOrDefault(p => p.Key == worklog.Author.Name);

                    worklogs.Add(new AtlassianWorklog
                    {
                        JiraWorklogId = worklog.Id,
                        InstalledInstanceId = instanceId,
                        IssueId = worklog.IssueId,
                        IssueKey = issue.Key,
                        Comment = worklog.Comment,
                        CreatedAt = worklog.CreatedAt,
                        UpdatedAt = worklog.UpdatedAt,
                        StartedAt = worklog.StartedAt,
                        TimeSpentInSeconds = worklog.TimeSpentSeconds,
                        AtlassianUserId = user.Id
                    });
                }
            }

            return worklogs;
        }

        private void UpdateDbUser(AtlassianUser dbUser, AtlassianUser jiraUser)
        {
            dbUser.DisplayName = jiraUser.DisplayName;
            dbUser.Key = jiraUser.Key;
            dbUser.EmailAddress = jiraUser.EmailAddress;
            dbUser.Avatar16x16 = jiraUser.Avatar16x16;
            dbUser.Avatar24x24 = jiraUser.Avatar24x24;
            dbUser.Avatar32x32 = jiraUser.Avatar32x32;
            dbUser.Avatar48x48 = jiraUser.Avatar48x48;
            dbUser.IsActive = jiraUser.IsActive;
        }

        private void UpdateDbWorklog(AtlassianWorklog dbWorklog, AtlassianWorklog jiraWorklog)
        {
            dbWorklog.Comment = jiraWorklog.Comment;
            dbWorklog.UpdatedAt = jiraWorklog.UpdatedAt;
            dbWorklog.TimeSpentInSeconds = jiraWorklog.TimeSpentInSeconds;
        }
        #endregion
    }
}
