﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Equilobe.DailyReport.Utils;

namespace Equilobe.DailyReport.SL
{
    public class ReportService : IReportService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public JiraRequestContext JiraRequestContext { get; set; }

        #region IReportService Implementation
        public List<DashboardItem> GetDashboardData(long instanceId)
        {
            var users = GetAllAtlassianUsers(instanceId);
            var usersIds = users.Select(p => p.Id).ToList();
            var worklogs = GetLastWorklogsByUsers(usersIds, instanceId);
            var avatarsFolderPath = ImageHelper.GetUserAvatarsRelativePath();
            var dashboardItems = new List<DashboardItem>();

            foreach (var user in users)
            {
                var item = ToDashboardItem(user, avatarsFolderPath);

                if (worklogs.ContainsKey(user.Id))
                    item.Worklogs.AddRange(GetLastWorklogsGroupForUser(worklogs, user));

                dashboardItems.Add(item);
            }

            return dashboardItems;
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
        private int GetAtlassianUsersCount(long instanceId)
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
                    .Where(p => p.InstalledInstanceId == instanceId);

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

            SyncAtlassianUserAvatars(users, instanceId);
            AddOrUpdateDbUsers(users, instanceId);
            SyncStallingUsers(instanceId);
        }

        private void SyncAtlassianUserAvatars(List<AtlassianUser> users, long instanceId)
        {
            var folderPath = ImageHelper.GetUserAvatarsFullPath();

            foreach (var user in users)
            {
                if (!user.IsActive)
                    continue;

                var image = JiraService.GetUserAvatar(JiraRequestContext, user.AvatarFileName);
                var imageName = user.Key + ".jpg";
                var path = Path.Combine(folderPath, imageName);

                try
                {
                    File.WriteAllBytes(path, image);
                    user.AvatarFileName = imageName;
                }
                catch
                {
                    user.AvatarFileName = null;
                }
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

        private void AddOrUpdateDbUsers(List<AtlassianUser> users, long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var dbUsers = db.AtlassianUsers.Where(p => p.InstalledInstanceId == instanceId);

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

        private void SyncStallingUsers(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var users = db.AtlassianUsers
                    .Where(p => p.IsActive)
                    .Where(p => p.InstalledInstanceId == instanceId);

                var usersIds = users.Select(p => p.Id).ToList();
                var worklogs = db.AtlassianWorklogs
                    .Where(p => usersIds.Contains(p.AtlassianUserId))
                    .GroupBy(p => p.AtlassianUserId)
                    .ToDictionary(p => p.Key, p => p.ToList());

                foreach (var user in users)
                {
                    if (!worklogs.ContainsKey(user.Id))
                    {
                        user.IsStalling = true;
                        continue;
                    }

                    var lastWorklog = worklogs[user.Id]
                        .OrderByDescending(p => p.StartedAt)
                        .Select(p => p.StartedAt)
                        .First();

                    if (lastWorklog < DateTime.UtcNow.AddMonths(-1))
                    {
                        user.IsStalling = true;
                        continue;
                    }

                    user.IsStalling = false;
                }

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
                    .Where(p => p.IsActive)
                    .Where(p => !p.IsStalling)
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

        private Dictionary<long, List<DashboardWorklog>> GetLastWorklogsByUsers(List<long> ids, long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var baseUrl = db.InstalledInstances
                    .Single(p => p.Id == instanceId)
                    .BaseUrl;

                return db.AtlassianWorklogs
                    .Where(p => ids.Contains(p.AtlassianUserId))
                    .OrderByDescending(p => p.StartedAt)
                    .Take(Constants.NumberOfDaysForWorklog * 2)
                    .GroupBy(p => p.AtlassianUserId)
                    .ToDictionary(p => p.Key, p => p.Select(q => ToDashboardWorklog(q, baseUrl)).ToList());
            }
        }

        private List<DashboardWorklogsGroup> GetLastWorklogsGroupForUser(Dictionary<long, List<DashboardWorklog>> worklogs, AtlassianUser user)
        {
            var userWorklogs = worklogs[user.Id];
            var worklogsGroupedByDay = GroupWorklogsByDay(userWorklogs);
            var lastBusinessDaysOfWork = GetLastBusinessDaysOfWork();
            var worklogGroups = new List<DashboardWorklogsGroup>();

            foreach (var worklogDay in lastBusinessDaysOfWork)
            {
                var worklogsList = worklogsGroupedByDay.ContainsKey(worklogDay) ? 
                    worklogsGroupedByDay[worklogDay] : 
                    new List<DashboardWorklog>();

                worklogGroups.Add(new DashboardWorklogsGroup
                {
                    Date = worklogDay,
                    WorklogGroup = worklogsList
                });
            }

            return worklogGroups;
        }

        private Dictionary<DateTime, List<DashboardWorklog>> GroupWorklogsByDay(List<DashboardWorklog> worklogs)
        {
            return worklogs
                .GroupBy(p => p.Date.Date)
                .ToDictionary(p => p.Key, p => p.ToList());
        }

        private List<DateTime> GetLastBusinessDaysOfWork()
        {
            var now = DateTime.UtcNow;

            var days = Enumerable
                .Range(0, Constants.NumberOfDaysForWorklog * 2)
                .Select(p => now.AddDays(-p).Date)
                .Where(p => p.DayOfWeek != DayOfWeek.Saturday && p.DayOfWeek != DayOfWeek.Sunday)
                .Take(Constants.NumberOfDaysForWorklog)
                .ToList();

            return days;
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

        private AtlassianUser ToAtlassianUser(JiraUser user, long instanceId)
        {
            return new AtlassianUser
            {
                DisplayName = user.DisplayName,
                InstalledInstanceId = instanceId,
                Key = user.Key,
                EmailAddress = user.EmailAddress,
                AvatarFileName = user.AvatarUrls.Big.AbsoluteUri,
                IsActive = user.IsActive
            };
        }

        private DashboardItem ToDashboardItem(AtlassianUser user, string avatarsFolderPath)
        {
            return new DashboardItem
            {
                AvatarUrl = avatarsFolderPath + "/" + user.AvatarFileName,
                DisplayName = user.DisplayName,
                Worklogs = new List<DashboardWorklogsGroup>()
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
            dbUser.AvatarFileName = jiraUser.AvatarFileName;
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
