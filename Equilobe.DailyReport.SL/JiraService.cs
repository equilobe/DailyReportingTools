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
        public Project GetProject(IJiraRequestContext context, long id)
        {
            return GetClient(context).GetProject(id);
        }

        public List<JiraIssue> GetTimesheetForUser(IJiraRequestContext context, DateTime startDate, DateTime endDate, string targetUser)
        {
            var client = GetClient(context);

            return new TimesheetGenerator(client).GetTimesheetIssuesForAuthor(targetUser, startDate, endDate);
        }

        public JiraUser GetUser(IJiraRequestContext context, string username)
        {
            return GetClient(context).GetUser(username);
        }

        public List<JiraUser> GetUsers(IJiraRequestContext context, string projectKey)
        {
            return GetClient(context).GetUsers(projectKey);
        }

        public RapidView GetRapidView(IJiraRequestContext context, string id)
        {
            return GetClient(context).GetRapidView(id);
        }

        public List<View> GetRapidViews(IJiraRequestContext context)
        {
            return GetClient(context).GetRapidViews();
        }

        public SprintReport GetSprintReport(IJiraRequestContext context, string rapidViewId, string sprintId)
        {
            return GetClient(context).GetSprintReport(rapidViewId, sprintId);
        }

        public List<Sprint> GetAllSprints(IJiraRequestContext context, string rapidViewId)
        {
            return GetClient(context).GetAllSprints(rapidViewId);
        }

        public JiraIssue GetIssue(IJiraRequestContext context, string issueKey)
        {
            return GetClient(context).GetIssue(issueKey);
        }

        public JiraIssues GetCompletedIssues(IJiraRequestContext context, DateTime startDate, DateTime endDate)
        {
            return GetClient(context).GetCompletedIssues(startDate, endDate);
        }

        public JiraIssues GetSprintTasks(IJiraRequestContext context, string projectKey)
        {
            return GetClient(context).GetSprintTasks(projectKey);
        }

        public ProjectInfo GetProjectInfo (IJiraRequestContext context, long id)
        {
            return GetClient(context).GetProjectInfo(id);
        }

        public List<ProjectInfo> GetProjectsInfo(IJiraRequestContext context)
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
                return new JiraClient(context.BaseUrl, context.SharedSecret);
            else
                return new JiraClient(context.BaseUrl, context.Username, context.Password);
        }
    }
}
