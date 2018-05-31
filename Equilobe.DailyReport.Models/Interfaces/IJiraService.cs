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
        JiraIssue GetIssue(JiraRequestContext context, string issueKey);
        JiraIssues GetCompletedIssues(IssuesContext context);
        JiraIssues GetSprintTasks(JiraRequestContext context, string projectKey, string sprintId);
        ProjectInfo GetProjectInfo(JiraRequestContext context, long id);
        List<ProjectInfo> GetProjectsInfo(JiraRequestContext context);
        byte[] GetUserAvatar(JiraRequestContext context, string url);
        SprintContext GetProjectSprintDetailsForDate(ProjectDateFilter filter);
    }
}
