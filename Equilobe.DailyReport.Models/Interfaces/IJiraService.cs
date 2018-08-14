using Equilobe.DailyReport.Models.Jira;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Policy;
using System;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraService : IService
    {
        bool CredentialsValid(object context, bool passwordEncrypted = true);
        JiraPolicy GetJiraInfo(ItemContext context);
        Project GetProject(JiraRequestContext context, long id);
        List<JiraIssue> GetTimesheetForUser(TimesheetContext context);
        List<JiraUser> GetAllUsers(JiraRequestContext context);
        JiraUser GetUser(JiraRequestContext context, string username);
        List<JiraUser> GetUsers(JiraRequestContext context, string projectKey);
        JiraIssue GetIssue(JiraRequestContext context, string issueKey);
        JiraIssues GetCompletedIssues(IssuesContext context);
        JiraIssues GetSprintTasks(JiraRequestContext context, string projectKey, string sprintId);
        ProjectInfo GetProjectInfo(JiraRequestContext context, long id);
        List<ProjectInfo> GetProjectsInfo(JiraRequestContext context);
        byte[] GetUserAvatar(JiraRequestContext context, string url);
        SprintContext GetProjectSprintDetailsForDate(ProjectDateFilter filter);
        List<JiraIssue> GetAllWorklogs(JiraRequestContext context, List<string> authors, DateTime fromDate, DateTime toDate);
    }
}
