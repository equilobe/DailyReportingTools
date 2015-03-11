using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class PolicySummaryService
    {
        public JiraRequestContext _requestContext { get; set; }

        public PolicySummaryService(JiraRequestContext context)
        {
            _requestContext = context;
        }

        public PolicySummary GetPolicySummary(long projectId)
        {
            var projectInfo = new JiraService(_requestContext).GetProjectInfo(projectId);
            return new PolicySummary
            {
                BaseUrl = _requestContext.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = new DataService().GetReportTime(_requestContext.BaseUrl, projectInfo.ProjectId)
            };
        }

        public List<PolicySummary> GetPoliciesSummary()
        {
            return new JiraService(_requestContext).GetProjectsInfo()
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = _requestContext.BaseUrl,
                    ProjectId = projectInfo.ProjectId,
                    ProjectKey = projectInfo.ProjectKey,
                    ProjectName = projectInfo.ProjectName,
                    ReportTime = new DataService().GetReportTime(_requestContext.BaseUrl, projectInfo.ProjectId)
                })
                .ToList();
        }
    }
}