using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class JiraService : IJiraService
    {
        public IEncryptionService EncryptionService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }

        public bool CredentialsValid(object context, bool passwordEncrypted = true)
        {
            var jiraRequestContext = new JiraRequestContext();
            context.CopyPropertiesOnObjects(jiraRequestContext);

            if (!passwordEncrypted)
                jiraRequestContext.JiraPassword = EncryptionService.Encrypt(jiraRequestContext.JiraPassword);

            try
            {
                GetUser(jiraRequestContext, jiraRequestContext.JiraUsername);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public JiraPolicy GetJiraInfo(ItemContext context)
        {
            long projectId;
            var jiraContext = new JiraRequestContext();

            using (var db = new ReportsDb())
            {
                var reportSettings = db.BasicSettings.SingleOrDefault(r => r.Id == context.Id);
                if (reportSettings == null)
                    return null;

                projectId = reportSettings.ProjectId;
                reportSettings.InstalledInstance.CopyPropertiesOnObjects(jiraContext);
            }

            var project = GetProject(jiraContext, projectId);
            var options = GetUsers(jiraContext, project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return new JiraPolicy
            {
                BaseUrl = jiraContext.BaseUrl,
                Username = jiraContext.JiraUsername,
                Password = jiraContext.JiraPassword,
                ProjectId = projectId,
                UserOptions = options
            };
        }

        public Project GetProject(JiraRequestContext context, long id)
        {
            return GetClient(context).GetProject(id);
        }

        public List<JiraIssue> GetTimesheetForUser(TimesheetContext context)
        {
            var client = GetClient(context.RequestContext);

            return new TimesheetGenerator(client).GetTimesheetIssuesForAuthor(context.ProjectKey, context.TargetUser, context.StartDate, context.EndDate);
        }

        public List<JiraUser> GetAllUsers(JiraRequestContext context)
        {
            return GetClient(context).GetAllUsers();
        }

        public List<JiraIssue> GetAllWorklogs(JiraRequestContext context, List<string> authors, DateTime fromDate, DateTime toDate)
        {
            var startDate = fromDate.ToString("yyyy-MM-dd");
            var endDate = toDate.ToString("yyyy-MM-dd");
            var worklogAuthors = string.Join(",", authors);

            return GetClient(context).GetAllWorklogs(worklogAuthors, startDate, endDate);
        }

        public JiraUser GetUser(JiraRequestContext context, string username)
        {
            return GetClient(context).GetUser(username);
        }

        public List<JiraUser> GetUsers(JiraRequestContext context, string projectKey)
        {
            return GetClient(context).GetUsers(projectKey);
        }

        public JiraIssue GetIssue(JiraRequestContext context, string issueKey)
        {
            return GetClient(context).GetIssue(issueKey);
        }

        public JiraIssues GetCompletedIssues(IssuesContext context)
        {
            return GetClient(context.RequestContext).GetCompletedIssues(context.ProjectKey, context.StartDate, context.EndDate);
        }

        public JiraIssues GetSprintTasks(JiraRequestContext context, string projectKey, string sprintId)
        {
            return GetClient(context).GetSprintTasks(projectKey, sprintId);
        }

        public ProjectInfo GetProjectInfo(JiraRequestContext context, long id)
        {
            return GetClient(context).GetProjectInfo(id);
        }

        public List<ProjectInfo> GetProjectsInfo(JiraRequestContext context)
        {
            return GetClient(context).GetProjectsInfo();
        }

        public SprintContext GetProjectSprintDetailsForDate(ProjectDateFilter filter)
        {
            var client = GetClient(filter.Context);
            return new SprintLoader(filter, client).GetSprintDetails();
        }

        public byte[] GetUserAvatar(JiraRequestContext context, string url)
        {
            var authorizationContext = new AuthorizationContext
            {
                Username = context.JiraUsername,
                Password = EncryptionService.Decrypt(context.JiraPassword),
                SharedSecret = context.SharedSecret,
                AddonKey = ConfigurationService.GetAddonKey()
            };

            return WebDownloads.GetUserAvatar(authorizationContext, url);
        }

        private JiraClient GetClient(IJiraRequestContext context)
        {
            if (!string.IsNullOrEmpty(context.SharedSecret))
                return JiraClient.CreateWithJwt(context.BaseUrl, context.SharedSecret, ConfigurationService.GetAddonKey());
            else
                return JiraClient.CreateWithBasicAuth(context.BaseUrl, context.JiraUsername, EncryptionService.Decrypt(context.JiraPassword));
        }
    }
}
