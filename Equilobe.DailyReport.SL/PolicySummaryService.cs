using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;
using System.Linq;
using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.SL
{
    public class PolicySummaryService : IPolicySummaryService
    {
        public IJiraRequestContextService JiraRequestContextService { get; set; }
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }


        public PolicySummary GetPolicySummary(long projectId)
        {
            var projectInfo = new JiraService().GetProjectInfo(projectId);
            return new PolicySummary
            {
                BaseUrl = JiraRequestContextService.Context.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = DataService.GetReportTime(JiraRequestContextService.Context.BaseUrl, projectInfo.ProjectId)
            };
        }

        public List<PolicySummary> GetPoliciesSummary()
        {
            return JiraService.GetProjectsInfo()
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = JiraRequestContextService.Context.BaseUrl,
                    ProjectId = projectInfo.ProjectId,
                    ProjectKey = projectInfo.ProjectKey,
                    ProjectName = projectInfo.ProjectName,
                    ReportTime = DataService.GetReportTime(JiraRequestContextService.Context.BaseUrl, projectInfo.ProjectId)
                })
                .ToList();
        }
    }
}