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
        public string _baseUrl { get; set; }
        public string _sharedSecret { get; set; }
        public JiraRequestContext _requestContext
        {
            get
            {
                return new JiraRequestContext(_baseUrl, _sharedSecret);
            }
        }

        public PolicySummaryService(string baseUrl, string sharedSecret)
        {
            _baseUrl = baseUrl;
            _sharedSecret = sharedSecret;
        }

        public PolicySummary GetPolicySummary(long projectId)
        {
            var projectInfo = new JiraService(_requestContext).GetProjectInfo(projectId);
            return new PolicySummary
            {
                BaseUrl = _baseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = new DataService().GetReportTime(_baseUrl, projectInfo.ProjectId)
            };
        }

        public List<PolicySummary> GetPoliciesSummary()
        {
            return new JiraService(_requestContext).GetProjectsInfo()
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = _baseUrl,
                    ProjectId = projectInfo.ProjectId,
                    ProjectKey = projectInfo.ProjectKey,
                    ProjectName = projectInfo.ProjectName,
                    ReportTime = new DataService().GetReportTime(_baseUrl, projectInfo.ProjectId)
                })
                .ToList();
        }
    }
}