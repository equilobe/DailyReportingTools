using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.DAL;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Storage;

namespace DailyReportWeb.Services
{
    public class PolicySummaryService
    {

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            var context = new ReportSettings { BaseUrl = baseUrl, SharedSecret = sharedSecret };
            
            return new JiraService().GetProjectsInfo(context)
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
    }
}