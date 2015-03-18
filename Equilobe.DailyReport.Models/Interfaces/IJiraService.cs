using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Jira.Filters;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraService
    {
        Project GetProject(long id);
        List<JiraIssue> GetTimesheetForUser(DateTime startDate, DateTime endDate, string targetUser);
        JiraUser GetUser(string username);
        List<JiraUser> GetUsers(string projectKey);
        RapidView GetRapidView(string id);
        List<View> GetRapidViews();
        SprintReport GetSprintReport(string rapidViewId, string sprintId);
        List<Sprint> GetAllSprints(string rapidViewId);
        JiraIssue GetIssue(string issueKey);
        JiraIssues GetCompletedIssues(DateTime startDate, DateTime endDate);
        JiraIssues GetSprintTasks(string projectKey);
        ProjectInfo GetProjectInfo(long id);
        List<ProjectInfo> GetProjectsInfo();
        Sprint GetProjectSprintForDate(ProjectDateFilter filter);
    }
}
