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
            // TODO (Vlad): refactor - send report settings id (form our DB) instead of jira project id

            var instance = GetInstanceWithReportSettings(context);

            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraRequestContext = new JiraRequestContext
            {
                BaseUrl = instance.BaseUrl,
                Password = instance.ReportSettings.First().Password,
                Username = instance.ReportSettings.First().Username
            };
            

            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, context.Id);
            return new ReportSettingsSummary
            {
                //BaseUrl = JiraRequestContextService.Context.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                //ReportTime = DataService.GetReportTime(JiraRequestContextService.Context.BaseUrl, projectInfo.ProjectId)
            };
        }

        public List<ReportSettingsSummary> GetPoliciesSummary(ItemContext context)
        {
            var instance = GetInstanceWithReportSettings(context);

            if (instance.UserId != context.UserId)
                throw new SecurityException();

            var jiraContext = new JiraRequestContext
            {
                BaseUrl = instance.BaseUrl,
                Password = instance.ReportSettings.First().Password,
                Username = instance.ReportSettings.First().Username                
            };

            return JiraService.GetProjectsInfo(jiraContext)
                    .Select(projectInfo => GetReportSettingsSummaryFromProjectInfo(projectInfo, instance))
                    .ToList();
        }

        private static ReportSettingsSummary GetReportSettingsSummaryFromProjectInfo(Models.Jira.ProjectInfo projectInfo, InstalledInstance instance)
        {
            return new ReportSettingsSummary
            {
                BaseUrl = instance.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = GetReportTimeForProjectIfSet(instance, projectInfo.ProjectId)
            };
        }

        private static string GetReportTimeForProjectIfSet(InstalledInstance instance, long projectId)
        {
            return instance.ReportSettings
                     .Where(r => r.ProjectId == projectId)
                     .Select(r => r.ReportTime)
                     .SingleOrDefault();
        }

        private static InstalledInstance GetInstanceWithReportSettings(ItemContext context)
        {            
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                                 .Include("ReportSettings")
                                 .Single(i => i.Id == context.Id);
            }
        }

    }
}