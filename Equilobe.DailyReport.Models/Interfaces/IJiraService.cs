using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraService : IService
    {
        bool CredentialsValid(object context, bool passwordEncrypted = true);
        JiraPolicy GetJiraInfo(ItemContext context);
        Project GetProject(JiraRequestContext context, long id);
        List<JiraIssue> GetTimesheetForUser(TimesheetContext context);
        JiraUser GetUser(JiraRequestContext context, string username);
        List<JiraUser> GetUsers(JiraRequestContext context, string projectKey);
        RapidView GetRapidView(JiraRequestContext context, string id);
        List<View> GetRapidViews(JiraRequestContext context);
        SprintReport GetSprintReport(JiraRequestContext context,string rapidViewId, string sprintId);
        List<Sprint> GetAllSprints(JiraRequestContext context, string rapidViewId);
        JiraIssue GetIssue(JiraRequestContext context, string issueKey);
        JiraIssues GetCompletedIssues(IssuesContext context);
        JiraIssues GetSprintTasks(JiraRequestContext context, string projectKey);
        ProjectInfo GetProjectInfo(JiraRequestContext context, long id);
        List<ProjectInfo> GetProjectsInfo(JiraRequestContext context);
        Sprint GetProjectSprintForDate(ProjectDateFilter filter);
        byte[] GetUserAvatar(JiraRequestContext context, string url);
    }
}
