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

        #region IJiraService Implementation
        public void UpdateDashboardData(long instanceId)
        {
            var projects = GetProjectsFromInstance(instanceId);
            var users = GetUsersFromProjects(projects)
                .Select(ToAtlassianUser)
                .ToList();

            using (var db = new ReportsDb())
            {
                var dbUsers = db.AtlassianUser.ToList();

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
        #endregion

        #region Helpers
        private List<AtlassianProject> GetProjectsFromInstance(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.BasicSettings
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Select(p => new AtlassianProject()
                    {
                        ProjectId = p.ProjectId,
                        BaseUrl = p.InstalledInstance.BaseUrl,
                        JiraPassword = p.InstalledInstance.JiraPassword,
                        JiraUsername = p.InstalledInstance.JiraUsername
                    })
                .ToList();
            }
        }

        private List<JiraUser> GetUsersFromProjects(List<AtlassianProject> instanceProjects)
        {
            var jiraContext = new JiraRequestContext();
            var users = new List<JiraUser>();

            foreach (var projectSettings in instanceProjects)
            {
                try
                {
                    projectSettings.CopyPropertiesOnObjects(jiraContext);
                    var project = JiraService.GetProject(jiraContext, projectSettings.ProjectId);
                    var projectUsers = JiraService.GetUsers(jiraContext, project.Key);

                    users.AddRange(projectUsers);
                }
                catch (Exception ex) { }
            }

            return RemoveUserDuplicates(users);
        }

        private List<JiraUser> RemoveUserDuplicates(List<JiraUser> users)
        {
            return users
                .GroupBy(p => p.emailAddress)
                .Select(p => p.First())
                .ToList();
        }

        private AtlassianUser ToAtlassianUser(JiraUser user)
        {
            return new AtlassianUser
            {
                DisplayName = user.displayName,
                Key = user.key,
                EmailAddress = user.emailAddress,
                Avatar16x16 = user.avatarUrls.VerySmall.AbsoluteUri,
                Avatar24x24 = user.avatarUrls.Small.AbsoluteUri,
                Avatar32x32 = user.avatarUrls.Med.AbsoluteUri,
                Avatar48x48 = user.avatarUrls.Big.AbsoluteUri
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
        }
        #endregion
    }
}
