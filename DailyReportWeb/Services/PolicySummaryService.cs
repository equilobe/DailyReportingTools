using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.DAL;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Models.ReportFrame;

namespace DailyReportWeb.Services
{
    public class PolicySummaryService
    {

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            var context = new JiraRequestContext { BaseUrl = baseUrl, SharedSecret = sharedSecret };
            
            return new JiraService().GetProjectsInfo(context)
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = baseUrl,
                    SharedSecret = sharedSecret,
                    ProjectId = projectInfo.ProjectId,
                    ProjectKey = projectInfo.ProjectKey,
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