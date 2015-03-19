using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.SL
{
    public class JiraService : IJiraService
    {
        public IJiraRequestContextService JiraRequestContextService { get; set; }


        public Project GetProject(long id)
        {
            return GetClient(JiraRequestContextService.Context).GetProject(id);
        }

        public List<JiraIssue> GetTimesheetForUser(DateTime startDate, DateTime endDate, string targetUser)
        {
            var client = GetClient(JiraRequestContextService.Context);

            return new TimesheetGenerator(client).GetTimesheetIssuesForAuthor(targetUser, startDate, endDate);
        }

        public JiraUser GetUser(string username)
        {
            return GetClient(JiraRequestContextService.Context).GetUser(username);
        }

        public List<JiraUser> GetUsers(string projectKey)
        {
            return GetClient(JiraRequestContextService.Context).GetUsers(projectKey);
        }

        public RapidView GetRapidView(string id)
        {
            return GetClient(JiraRequestContextService.Context).GetRapidView(id);
        }

        public List<View> GetRapidViews()
        {
            return GetClient(JiraRequestContextService.Context).GetRapidViews();
        }

        public SprintReport GetSprintReport(string rapidViewId, string sprintId)
        {
            return GetClient(JiraRequestContextService.Context).GetSprintReport(rapidViewId, sprintId);
        }

        public List<Sprint> GetAllSprints(string rapidViewId)
        {
            return GetClient(JiraRequestContextService.Context).GetAllSprints(rapidViewId);
        }

        public JiraIssue GetIssue(string issueKey)
        {
            return GetClient(JiraRequestContextService.Context).GetIssue(issueKey);
        }

        public JiraIssues GetCompletedIssues(DateTime startDate, DateTime endDate)
        {
            return GetClient(JiraRequestContextService.Context).GetCompletedIssues(startDate, endDate);
        }

        public JiraIssues GetSprintTasks(string projectKey)
        {
            return GetClient(JiraRequestContextService.Context).GetSprintTasks(projectKey);
        }

        public ProjectInfo GetProjectInfo (long id)
        {
            return GetClient(JiraRequestContextService.Context).GetProjectInfo(id);
        }

        public List<ProjectInfo> GetProjectsInfo(JiraRequestContext context)
        {
            return GetClient(context).GetProjectsInfo();
        }

        public Sprint GetProjectSprintForDate(ProjectDateFilter filter)
        {
            var client = GetClient(filter.Context);
            return new SprintLoader(filter, client).GetLatestSprint();
        }

        private JiraClient GetClient(IJiraRequestContext context)
        {
            if (!string.IsNullOrEmpty(context.SharedSecret))
                return JiraClient.CreateWithJwt(context.BaseUrl, context.SharedSecret, "addonKey");
            else
                return JiraClient.CreateWithBasicAuth(context.BaseUrl, context.Username, context.Password);
        }
    }
}
