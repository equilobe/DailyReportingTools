using Equilobe.DailyReport.DAL;
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

        public JiraRequestContext JiraRequestContext { get; set; }

        #region IReportService Implementation
        public void UpdateDashboardData(long instanceId)
        {
            JiraRequestContext = GetJiraRequestContext(instanceId);

            SyncAtlassianUsers(instanceId);
            SyncAtlassianWorklogs(instanceId);
        }
        #endregion

        #region Helpers
        private void SyncAtlassianWorklogs(long instanceId)
        {
            SyncDeletedWorklogs(instanceId);
            SyncUpdatedWorklogs(instanceId);
        }

        private void SyncDeletedWorklogs(long instanceId)
        {
            var deletedWorklogsIds = JiraService.GetDeletedWorklogsIds(JiraRequestContext, DateTime.UtcNow.AddMonths(-7));

            if (deletedWorklogsIds == null)
                return;

            using (var db = new ReportsDb())
            {
                var dbWorklogs = db.AtlassianWorklog
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();

                foreach (var worklogId in deletedWorklogsIds)
                {
                    var dbWorklog = dbWorklogs.SingleOrDefault(p => p.JiraWorklogId == worklogId);

                    if (dbWorklog != null)
                        db.AtlassianWorklog.Remove(dbWorklog);
                }

                db.SaveChanges();
            }
        }

        private void SyncUpdatedWorklogs(long instanceId)
        {
            var worklogs = GetAtlassianWorklogs(instanceId);

            using (var db = new ReportsDb())
            {
                var dbWorklogs = db.AtlassianWorklog
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();

                foreach (var worklog in worklogs)
                {
                    var dbWorklog = dbWorklogs.Where(p => p.JiraWorklogId == worklog.JiraWorklogId).SingleOrDefault();

                    if (dbWorklog == null)
                        db.AtlassianWorklog.Add(worklog);
                    else
                        UpdateDbWorklog(dbWorklog, worklog);
                }

                db.SaveChanges();
            }
        }

        private List<AtlassianWorklog> GetAtlassianWorklogs(long instanceId)
        {
            var users = GetAllAtlassianUsers(instanceId);
            var userKeys = users
                .Select(p => p.Key)
                .ToList();

            var issueWorklogs = JiraService.GetWorklogsForMultipleUsers(JiraRequestContext, userKeys, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);
            var worklogs = GetWorklogsFromIssueWorklogs(issueWorklogs, users, instanceId);

            return worklogs;
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
                        db.AtlassianUser.Add(user);
                    else
                        UpdateDbUser(dbUser, user);
                }

                db.SaveChanges();
            }
        }

        private List<AtlassianUser> GetAllAtlassianUsers(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianUser
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();
            }
        }

        private JiraRequestContext GetJiraRequestContext(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var instance = db.InstalledInstances.Single(p => p.Id == instanceId);
                var jiraRequestContext = new JiraRequestContext();

                instance.CopyPropertiesOnObjects(jiraRequestContext);

                return jiraRequestContext;
            }
        }

        private AtlassianUser ToAtlassianUser(JiraUser user, long instanceId)
        {
            return new AtlassianUser
            {
                DisplayName = user.displayName,
                InstalledInstanceId = instanceId,
                Key = user.key,
                EmailAddress = user.emailAddress,
                Avatar16x16 = user.avatarUrls.VerySmall.AbsoluteUri,
                Avatar24x24 = user.avatarUrls.Small.AbsoluteUri,
                Avatar32x32 = user.avatarUrls.Med.AbsoluteUri,
                Avatar48x48 = user.avatarUrls.Big.AbsoluteUri,
                IsActive = user.active
            };
        }

        private List<AtlassianWorklog> GetWorklogsFromIssueWorklogs(List<JiraIssue> issueWorklog, List<AtlassianUser> users, long instanceId)
        {
            var worklogs = new List<AtlassianWorklog>();

            foreach (var issue in issueWorklog)
            {
                foreach (var worklog in issue.fields.worklog.worklogs)
                {
                    var user = users.SingleOrDefault(p => p.Key == worklog.author.name);

                    worklogs.Add(new AtlassianWorklog
                    {
                        JiraWorklogId = worklog.id,
                        InstalledInstanceId = instanceId,
                        IssueId = worklog.issueId,
                        IssueKey = issue.key,
                        Comment = worklog.comment,
                        CreatedAt = DateTime.Parse(worklog.created),
                        UpdatedAt = DateTime.Parse(worklog.updated),
                        StartedAt = DateTime.Parse(worklog.started),
                        TimeSpentInSeconds = worklog.timeSpentSeconds,
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
