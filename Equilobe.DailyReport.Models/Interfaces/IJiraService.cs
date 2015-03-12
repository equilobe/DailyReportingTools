using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Jira.Filters;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraService
    {
        Project GetProject(long id);
        List<JiraIssue> GetTimesheetForUser(string projectKey, string targetUser, DateTime startDate, DateTime endDate);
        JiraUser GetUser(string username);
        List<JiraUser> GetUsers(string projectKey);
        RapidView GetRapidView(string id);
        List<View> GetRapidViews();
        SprintReport GetSprintReport(string rapidViewId, string sprintId);
        List<Sprint> GetAllSprints(string rapidViewId);
        JiraIssue GetIssue(string issueKey);
        JiraIssues GetCompletedIssues(string projectKey, DateTime startDate, DateTime endDate);
        JiraIssues GetSprintTasks(string projectKey);
        List<ProjectInfo> GetProjectsInfo();
        Sprint GetProjectSprintForDate(ProjectDateFilter filter);
    }
}
