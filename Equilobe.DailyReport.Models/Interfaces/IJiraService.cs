﻿using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.Models.ReportFrame;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraService : IService
    {
        Project GetProject(JiraRequestContext context, long id);
        List<JiraIssue> GetTimesheetForUser(JiraRequestContext context, DateTime startDate, DateTime endDate, string targetUser);
        JiraUser GetUser(JiraRequestContext context, string username);
        List<JiraUser> GetUsers(JiraRequestContext context, string projectKey);
        RapidView GetRapidView(JiraRequestContext context, string id);
        List<View> GetRapidViews(JiraRequestContext context);
        SprintReport GetSprintReport(JiraRequestContext context,string rapidViewId, string sprintId);
        List<Sprint> GetAllSprints(JiraRequestContext context, string rapidViewId);
        JiraIssue GetIssue(JiraRequestContext context, string issueKey);
        JiraIssues GetCompletedIssues(JiraRequestContext context, DateTime startDate, DateTime endDate);
        JiraIssues GetSprintTasks(JiraRequestContext context, string projectKey);
        ProjectInfo GetProjectInfo(JiraRequestContext context, long id);
        List<ProjectInfo> GetProjectsInfo(JiraRequestContext context);
        Sprint GetProjectSprintForDate(ProjectDateFilter filter);
    }
}
