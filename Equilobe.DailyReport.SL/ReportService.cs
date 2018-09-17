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
using Equilobe.DailyReport.Utils;

namespace Equilobe.DailyReport.SL
{
    public class ReportService : IReportService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public IAtlassianUserDataService AtlassianUserDataService { get; set; }
        public IAtlassianWorklogDataService AtlassianWorklogDataService { get; set; }

        #region IReportService Implementation
        public bool IsDashboardAvailable(DashboardFilter filter)
        {
            var instance = DataService.GetInstance(filter.InstanceId);

            if (filter.IsAuthenticated && (string.IsNullOrEmpty(filter.Hash) || instance.Hash != filter.Hash))
                return false;

            return true;
        }

        public DashboardPage GetDashboardData(long instanceId)
        {
            return new DashboardPage
            {
                IsAvailable = true,
                Items = GetDashboardItems(instanceId)
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
            var reportContext = GetReportContext(instanceId);

            SyncAtlassianUsers(reportContext);
            SyncAtlassianWorklogs(reportContext);
            UpdateLastSyncDate(reportContext.InstanceId);

            CreateOrUpdateSyncScheduleTask(reportContext.InstanceId);
        }
        #endregion

        #region Update methods
        private void SyncAtlassianUsers(ReportContext context)
        {
            var users = JiraService.GetAllUsers(context.JiraRequestContext)
                .Select(p => ToAtlassianUser(p, context.InstanceId))
                .ToList();

            AtlassianUserDataService.SyncAtlassianUsers(users, context);
        }

        private void SyncAtlassianWorklogs(ReportContext context)
        {
            var deletedWorklogsIds = JiraService.GetDeletedWorklogsIds(context.JiraRequestContext, context.BusinessDaysAgo);
            var jiraWorklogs = GetAtlassianWorklogs(context);

            AtlassianWorklogDataService.SyncAtlassianWroklogs(jiraWorklogs, deletedWorklogsIds, context.InstanceId);
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
        #endregion

        #region Helpers
        private List<DashboardItem> GetDashboardItems(long instanceId)
        {
            var users = AtlassianUserDataService.GetAtlassianUsers(instanceId, true, false);
            var reportContext = GetReportContext(instanceId);
            var worklogs = GetLastWorklogsByUsers(users, reportContext);
            var avatarsFolderPath = ImageHelper.GetUserAvatarsRelativePath();
            var dashboardItems = new List<DashboardItem>();

            foreach (var user in users)
            {
                var item = ToDashboardItem(user, avatarsFolderPath);

                item.Worklogs.AddRange(GetLastWorklogsGroupForUser(worklogs, user, reportContext));

                dashboardItems.Add(item);
            }

            return dashboardItems;
        }

        private ReportContext GetReportContext(long instanceId)
        {
            var offsetFromUtc = DataService.GetOffsetFromInstanceId(instanceId);

            return new ReportContext
            {
                InstanceId = instanceId,
                BusinessDaysAgo =  GetLastBusinessDaysAgo(Constants.NumberOfDaysForWorklog, offsetFromUtc),
                OffsetFromUtc = offsetFromUtc,
                JiraRequestContext = GetJiraRequestContext(instanceId)
            };
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

        private List<AtlassianWorklog> GetAtlassianWorklogs(ReportContext context)
        {
            var users = AtlassianUserDataService.GetAtlassianUsers(context.InstanceId, true);
            var userKeys = users
                .Select(p => p.Key)
                .ToList();

            var issueWorklogs = JiraService.GetWorklogsForMultipleUsers(context.JiraRequestContext, userKeys, context.BusinessDaysAgo);
            var worklogs = GetWorklogsFromIssueWorklogs(issueWorklogs, users, context.InstanceId);

            return worklogs;
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

        private Dictionary<long, List<DashboardWorklog>> GetLastWorklogsByUsers(List<AtlassianUser> users, ReportContext context)
        {
            using (var db = new ReportsDb())
            {
                var ids = users.Select(p => p.Id).ToList();

                var baseUrl = db.InstalledInstances
                    .Single(p => p.Id == context.InstanceId)
                    .BaseUrl;

                return AtlassianWorklogDataService.GetLastWorklogsByUsers(ids, context, baseUrl);
            }
        }

        private List<DayWorklogGroup> GetLastWorklogsGroupForUser(Dictionary<long, List<DashboardWorklog>> userWorklogs, AtlassianUser user, ReportContext filter)
        {
            var worklogsGroupedByDay = userWorklogs.ContainsKey(user.Id) ? GroupWorklogsByDay(userWorklogs[user.Id]) : null;
            var lastBusinessDaysOfWork = GetLastBusinessDaysOfWork(filter);
            var worklogGroups = new List<DayWorklogGroup>();

            foreach (var worklogDay in lastBusinessDaysOfWork)
            {
                var worklogsList = worklogsGroupedByDay != null && worklogsGroupedByDay.ContainsKey(worklogDay) ?
                    worklogsGroupedByDay[worklogDay] :
                    new List<DashboardWorklog>();

                worklogGroups.Add(new DayWorklogGroup
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

        private List<DateTime> GetLastBusinessDaysOfWork(ReportContext filter)
        {
            var yesterday = DateTime.Today.AddDays(-1).ToOriginalTimeZone(filter.OffsetFromUtc);
            var numberOfDays = yesterday.Subtract(filter.BusinessDaysAgo).Days;

            var days = Enumerable
                .Range(0, numberOfDays)
                .Select(p => yesterday.AddDays(-p).Date)
                .Where(p => p.DayOfWeek != DayOfWeek.Saturday && p.DayOfWeek != DayOfWeek.Sunday)
                .Take(Constants.NumberOfDaysForWorklog)
                .ToList();

            return days;
        }

        private DateTime GetLastBusinessDaysAgo(int days, TimeSpan offsetFromUtc)
        {
            var tempDate = DateTime.Today.ToOriginalTimeZone(offsetFromUtc);

            while (days > 0)
            {
                tempDate = tempDate.AddDays(-1);

                if (tempDate.DayOfWeek != DayOfWeek.Saturday && tempDate.DayOfWeek != DayOfWeek.Sunday)
                    days--;
            }

            return tempDate;
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
                Worklogs = new List<DayWorklogGroup>()
            };
        }

        private List<AtlassianWorklog> GetWorklogsFromIssueWorklogs(List<JiraIssue> issueWorklog, List<AtlassianUser> users, long instanceId)
        {
            var worklogs = new List<AtlassianWorklog>();

            foreach (var issue in issueWorklog)
            {
                foreach (var worklog in issue.Fields.Worklog.Worklogs)
                {
                    var user = users.SingleOrDefault(p => p.Key == worklog.Author.Key);

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
                        TimeSpentInSeconds = ToRoundedTime(worklog.TimeSpentSeconds),
                        AtlassianUserId = user.Id
                    });
                }
            }

            return worklogs;
        }

        private long ToRoundedTime(long seconds)
        {
            if (seconds % 60 == 0)
                return seconds;

            return (long)Math.Round(seconds / 60d, 0) * 60;
        }
        #endregion
    }
}
