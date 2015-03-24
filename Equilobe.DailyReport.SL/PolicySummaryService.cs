using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Equilobe.DailyReport.SL
{
    public class PolicySummaryService : IPolicySummaryService
    {
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }

        public ReportSettingsSummary GetPolicySummary(ItemContext context)
        {
            var reportSettings = new ReportsDb().ReportSettings
                                                .Where(rs => rs.Id == context.Id)
                                                .Single();

            var instance = reportSettings.InstalledInstance;
            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraRequestContext = new JiraRequestContext();
            instance.CopyPropertiesOnObjects(jiraRequestContext);

            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, reportSettings.ProjectId);
            return new ReportSettingsSummary
            {
                Id = context.Id,
                BaseUrl = instance.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = GetReportSettingsReportTime(instance, projectInfo.ProjectId)
            };
        }

        public List<ReportSettingsSummary> GetPoliciesSummary(ItemContext context)
        {
            var instance = new ReportsDb().InstalledInstances
                                          .Where(i => i.Id == context.Id)
                                          .Single();

            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraRequestContext = new JiraRequestContext();
            instance.CopyPropertiesOnObjects(jiraRequestContext);

            return JiraService.GetProjectsInfo(jiraRequestContext)
                              .Select(projectInfo => new ReportSettingsSummary
                              {
                                  Id = GetReportSettingsId(instance, projectInfo.ProjectId),
                                  BaseUrl = instance.BaseUrl,
                                  ProjectId = projectInfo.ProjectId,
                                  ProjectKey = projectInfo.ProjectKey,
                                  ProjectName = projectInfo.ProjectName,
                                  ReportTime = GetReportSettingsReportTime(instance, projectInfo.ProjectId)
                              })
                              .ToList();
        }

        private static long GetReportSettingsId(InstalledInstance instance, long projectId)
        {
            return instance.ReportSettings
                           .Where(r => r.ProjectId == projectId)
                           .Select(r => r.Id)
                           .SingleOrDefault();
        }

        private static string GetReportSettingsReportTime(InstalledInstance instance, long projectId)
        {
            return instance.ReportSettings
                           .Where(r => r.ProjectId == projectId)
                           .Select(r => r.ReportTime)
                           .SingleOrDefault();
        }
    }
}