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
        public static PolicySummary GetPolicySummary(string baseUrl, string sharedSecret, long projectId)
        {
            var context = new JiraRequestContext(baseUrl, sharedSecret);

            var projectInfo = new JiraService().GetProjectInfo(context, projectId);
            return new PolicySummary
            {
                BaseUrl = baseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = GetReportTime(baseUrl, projectInfo.ProjectId)
            };
        }

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            var context = new JiraRequestContext(baseUrl, sharedSecret);

            return new JiraService().GetProjectsInfo(context)
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = baseUrl,
                    ProjectId = projectInfo.ProjectId,
                    ProjectKey = projectInfo.ProjectKey,
                    ProjectName = projectInfo.ProjectName,
                    ReportTime = GetReportTime(baseUrl, projectInfo.ProjectId)
                })
                .ToList();
        }

        public static string GetReportTime(string baseUrl, long projectId)
        {
            return new ReportsDb().ReportSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
        }
    }
}