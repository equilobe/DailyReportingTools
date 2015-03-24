using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;
using System.Linq;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models;
using System.Security;
using Equilobe.DailyReport.Models.Storage;

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
                         .SingleOrDefault();
            if (reportSettings == null)
                return null;

            var instance = reportSettings.InstalledInstance;

            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraRequestContext = new JiraRequestContext
            {
                BaseUrl = instance.BaseUrl,
                Password = instance.JiraPassword,
                Username = instance.JiraUsername
            };

            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, reportSettings.ProjectId);
            return new ReportSettingsSummary
            {
                Id = GetProjectId(instance, projectInfo.ProjectId),
                BaseUrl = instance.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = GetProjectReportTime(instance, projectInfo.ProjectId)
            };
        }

        public List<ReportSettingsSummary> GetPoliciesSummary(ItemContext context)
        {
            var instance = new ReportsDb().InstalledInstances
                         .Where(i => i.Id == context.Id)
                         .Single();

            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraRequestContext = new JiraRequestContext
            {
                BaseUrl = instance.BaseUrl,
                Password = instance.JiraPassword,
                Username = instance.JiraUsername
            };

            return JiraService.GetProjectsInfo(jiraRequestContext)
                    .Select(projectInfo => new ReportSettingsSummary
                    {
                        Id = GetProjectId(instance, projectInfo.ProjectId),
                        BaseUrl = instance.BaseUrl,
                        ProjectId = projectInfo.ProjectId,
                        ProjectKey = projectInfo.ProjectKey,
                        ProjectName = projectInfo.ProjectName,
                        ReportTime = GetProjectReportTime(instance, projectInfo.ProjectId)
                    })
                    .ToList();
        }

        private static long GetProjectId(InstalledInstance instance, long projectId)
        {
            return instance.ReportSettings
                     .Where(r => r.ProjectId == projectId)
                     .Select(r => r.Id)
                     .SingleOrDefault();
        }

        private static string GetProjectReportTime(InstalledInstance instance, long projectId)
        {
            return instance.ReportSettings
                     .Where(r => r.ProjectId == projectId)
                     .Select(r => r.ReportTime)
                     .SingleOrDefault();
        }
    }
}