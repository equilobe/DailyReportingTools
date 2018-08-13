using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
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
        }
        #endregion

        #region Helpers
        private void UpdateAtlassianUsers(long instanceId)
        {
            var jiraRequestContext = GetJiraRequestContext(instanceId);
            var users = JiraService.GetAllUsers(jiraRequestContext)
                .Select(p => ToAtlassianUser(p, instanceId))
                .ToList();

            using (var db = new ReportsDb())
            {
                var dbUsers = db.AtlassianUser
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .ToList();

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
