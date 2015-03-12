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
        public JiraRequestContext _jiraRequestContext { get; set; }

        public JiraService()
        {

        }

        public JiraService(JiraRequestContext jiraRequestContext)
        {
            _jiraRequestContext = jiraRequestContext;
        }
        public Project GetProject(long id)
        {
            return GetClient(_jiraRequestContext).GetProject(id);
        }

        public List<JiraIssue> GetTimesheetForUser(string projectKey, string targetUser, DateTime startDate, DateTime endDate)
        {
            var client = GetClient(_jiraRequestContext);

            return new TimesheetGenerator(client).GetTimesheetIssuesForAuthor(projectKey, targetUser, startDate, endDate);
        }

        public JiraUser GetUser(string username)
        {
            return GetClient(_jiraRequestContext).GetUser(username);
        }

        public List<JiraUser> GetUsers(string projectKey)
        {
            return GetClient(_jiraRequestContext).GetUsers(projectKey);
        }

        public RapidView GetRapidView(string id)
        {
            return GetClient(_jiraRequestContext).GetRapidView(id);
        }

        public List<View> GetRapidViews()
        {
            return GetClient(_jiraRequestContext).GetRapidViews();
        }

        public SprintReport GetSprintReport(string rapidViewId, string sprintId)
        {
            return GetClient(_jiraRequestContext).GetSprintReport(rapidViewId, sprintId);
        }

        public List<Sprint> GetAllSprints(string rapidViewId)
        {
            return GetClient(_jiraRequestContext).GetAllSprints(rapidViewId);
        }

        public JiraIssue GetIssue(string issueKey)
        {
            return GetClient(_jiraRequestContext).GetIssue(issueKey);
        }

        public JiraIssues GetCompletedIssues(string projectKey, DateTime startDate, DateTime endDate)
        {
            return GetClient(_jiraRequestContext).GetCompletedIssues(projectKey, startDate, endDate);
        }

        public JiraIssues GetSprintTasks(string projectKey)
        {
            return GetClient(_jiraRequestContext).GetSprintTasks(projectKey);
        }

        public ProjectInfo GetProjectInfo (long id)
        {
            return GetClient(_jiraRequestContext).GetProjectInfo(id);
        }

        public List<ProjectInfo> GetProjectsInfo()
        {
            return GetClient(_jiraRequestContext).GetProjectsInfo();
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
