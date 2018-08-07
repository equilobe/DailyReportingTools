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
            using (var db = new ReportsDb())
            {
                var users = GetUsersFromInstance(db, instanceId);

                //TODO - add in DB 
            }
        }
        #endregion

        #region Helpers
        private List<JiraUser> GetUsersFromInstance(ReportsDb db, long instanceId)
        {
            var basicSettings = db.BasicSettings.Where(p => p.InstalledInstanceId == instanceId).Select(p => new { p.ProjectId, p.InstalledInstance }).ToList();
            var jiraContext = new JiraRequestContext();
            var users = new List<JiraUser>();

            foreach (var setting in basicSettings)
            {
                try
                {
                    setting.InstalledInstance.CopyPropertiesOnObjects(jiraContext);
                    var project = JiraService.GetProject(jiraContext, setting.ProjectId);
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
        #endregion
    }
}
