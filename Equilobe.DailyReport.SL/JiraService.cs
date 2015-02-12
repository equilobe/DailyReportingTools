using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Report;
using Equilobe.DailyReport.Models.Services;
using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.Models.Jira;

namespace Equilobe.DailyReport.SL
{
    public class JiraService : IJiraService
    {
        public Project GetProject(IJiraRequestContext context, string id)
        {
            return GetClient(context).GetProject(id);
        }

        public Timesheet GetTimesheetForUser(IJiraRequestContext context, DateTime startDate, DateTime endDate, string targetUser)
        {
            return GetClient(context).GetTimesheetForUser(startDate, endDate, targetUser);
        }

        public Timesheet GetTimesheet(IJiraRequestContext context, DateTime startDate, DateTime endDate)
        {
            return GetClient(context).GetTimesheet(startDate, endDate);
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

        public List<ProjectInfo> GetProjectsInfo(IJiraRequestContext context)
        {
            return GetClient(context).GetProjectsInfo();
        }

        private JiraClient GetClient(IJiraRequestContext context)
        {
            if (context.SharedSecret != null)
                return new JiraClient(context.BaseUrl, context.SharedSecret);
            else
                return new JiraClient(context.BaseUrl, context.Username, context.Password);
        }
    }
}
