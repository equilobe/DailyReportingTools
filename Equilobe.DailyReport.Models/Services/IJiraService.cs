using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Report;
using Equilobe.DailyReport.Models.Jira;

namespace Equilobe.DailyReport.Models.Services
{
    public interface IJiraService
    {
        Project GetProject(IJiraRequestContext context, string id);
        Timesheet GetTimesheetForUser(IJiraRequestContext context, DateTime startDate, DateTime endDate, string targetUser);
        Timesheet GetTimesheet(IJiraRequestContext context, DateTime startDate, DateTime endDate);
        JiraUser GetUser(IJiraRequestContext context, string username);
        List<JiraUser> GetUsers(IJiraRequestContext context, string projectKey);
        RapidView GetRapidView(IJiraRequestContext context, string id);
        List<View> GetRapidViews(IJiraRequestContext context);
        SprintReport GetSprintReport(IJiraRequestContext context, string rapidViewId, string sprintId);
        List<Sprint> GetAllSprints(IJiraRequestContext context, string rapidViewId);
        JiraIssue GetIssue(IJiraRequestContext context, string issueKey);
        JiraIssues GetCompletedIssues(IJiraRequestContext context, DateTime startDate, DateTime endDate);
        JiraIssues GetSprintTasks(IJiraRequestContext context, string projectKey);
        List<ProjectInfo> GetProjectsInfo(IJiraRequestContext context);
    }
}
