using Equilobe.DailyReport.Models.ReportPolicy;
using Equilobe.DailyReport.DAL;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Services
{
    public class PolicySummaryService
    {

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            return GetProjectsInfo(baseUrl, sharedSecret)
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = baseUrl,
                    SharedSecret = sharedSecret,
                    ProjectId = projectInfo.ProjectId,
                    ProjectName = projectInfo.ProjectName,
                    ReportTime = GetReportTime(baseUrl, projectInfo.ProjectId)
                })
                .ToList();
        }

        private static string GetReportTime(string baseUrl, long projectId)
        {
            return new ReportsDb().ReportSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
        }

        private static List<ProjectInfo> GetProjectsInfo(string baseUrl, string sharedSecret)
        {
            var policy = new JiraPolicy
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret
            };

            var request = new RestRequest(JiraReporter.JiraApiUrls.Projects(), Method.GET);

            return JiraReporter.RestApiRequests.ResolveRequest<List<ProjectInfo>>(policy, request);
        }
    }
}