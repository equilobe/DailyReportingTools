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
        public Project GetProject(JiraRequestContext context, long id)
        {
            return GetClient(context).GetProject(id);
        }

        public List<JiraIssue> GetTimesheetForUser(JiraRequestContext context, DateTime startDate, DateTime endDate, string targetUser)
        {
            var client = GetClient(context);

            return new TimesheetGenerator(client).GetTimesheetIssuesForAuthor(targetUser, startDate, endDate);
        }

        public JiraUser GetUser(JiraRequestContext context, string username)
        {
            return GetClient(context).GetUser(username);
        }

        public List<JiraUser> GetUsers(JiraRequestContext context, string projectKey)
        {
            return GetClient(context).GetUsers(projectKey);
        }

        public RapidView GetRapidView(JiraRequestContext context, string id)
        {
            return GetClient(context).GetRapidView(id);
        }

        public List<View> GetRapidViews(JiraRequestContext context)
        {
            return GetClient(context).GetRapidViews();
        }

        public SprintReport GetSprintReport(JiraRequestContext context,string rapidViewId, string sprintId)
        {
            return GetClient(context).GetSprintReport(rapidViewId, sprintId);
        }

        public List<Sprint> GetAllSprints(JiraRequestContext context,string rapidViewId)
        {
            return GetClient(context).GetAllSprints(rapidViewId);
        }

        public JiraIssue GetIssue(JiraRequestContext context,string issueKey)
        {
            return GetClient(context).GetIssue(issueKey);
        }

        public JiraIssues GetCompletedIssues(JiraRequestContext context,DateTime startDate, DateTime endDate)
        {
            return GetClient(context).GetCompletedIssues(startDate, endDate);
        }

        public JiraIssues GetSprintTasks(JiraRequestContext context,string projectKey)
        {
            return GetClient(context).GetSprintTasks(projectKey);
        }

        public ProjectInfo GetProjectInfo (JiraRequestContext context,long id)
        {
            return GetClient(context).GetProjectInfo(id);
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
