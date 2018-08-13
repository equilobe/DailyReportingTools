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

        #region IReportService Implementation
        public void UpdateDashboardData(long instanceId)
        {
            UpdateAtlassianUsers(instanceId);
            UpdateUsersWorklogs(instanceId);
        }
        #endregion

        #region Helpers
        private void UpdateUsersWorklogs(long instanceId)
        {
            var jiraRequestContext = GetJiraRequestContext(instanceId);
            var users = GetAllAtlassianUsers(instanceId);
            var userKeys = users
                .Select(p => p.Key)
                .ToList();

            var issueWorklogs = JiraService.GetAllWorklogs(jiraRequestContext, userKeys, DateTime.UtcNow.AddMonths(-1), DateTime.UtcNow);
            var worklogs = GetWorklogsFromIssueWorklogs(issueWorklogs, users);

            //TODO update db table
        }

        private void UpdateAtlassianUsers(long instanceId)
        {
            var jiraRequestContext = GetJiraRequestContext(instanceId);
            var users = JiraService.GetAllUsers(jiraRequestContext)
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

        private List<AtlassianWorklog> GetWorklogsFromIssueWorklogs(List<JiraIssueWorklog> issueWorklog, List<AtlassianUser> users)
        {
            var worklogs = new List<AtlassianWorklog>();

            foreach (var issue in issueWorklog)
            {
                foreach (var worklog in issue.Fields.worklog.worklogs)
                {
                    var user = users.SingleOrDefault(p => p.Key == worklog.author.name);

                    worklogs.Add(new AtlassianWorklog()
                    {
                        JiraWorklogId = Int64.Parse(worklog.id),
                        IssueId = worklog.issueId,
                        IssueKey = issue.Key,
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

        private void UpdateDbUser(AtlassianUser dbUser, AtlassianUser updatedUser)
        {
            dbUser.DisplayName = updatedUser.DisplayName;
            dbUser.Key = updatedUser.Key;
            dbUser.EmailAddress = updatedUser.EmailAddress;
            dbUser.Avatar16x16 = updatedUser.Avatar16x16;
            dbUser.Avatar24x24 = updatedUser.Avatar24x24;
            dbUser.Avatar32x32 = updatedUser.Avatar32x32;
            dbUser.Avatar48x48 = updatedUser.Avatar48x48;
            dbUser.IsActive = updatedUser.IsActive;
        }
        #endregion
    }
}
