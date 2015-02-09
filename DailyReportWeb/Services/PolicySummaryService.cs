using Equilobe.DailyReport.Models.ReportPolicy;
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
                    Id = projectInfo.Id,
                    Name = projectInfo.Name
                })
                .ToList();
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