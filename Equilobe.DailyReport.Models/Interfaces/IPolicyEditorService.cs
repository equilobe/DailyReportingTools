using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicyEditorService : IService
    {
        FullReportSettings GetPolicy(ItemContext context);
        ReportPolicy GetPolicyDetails(string baseUrl, long projectId);
        FullReportSettings SyncPolicy(Equilobe.DailyReport.Models.Policy.JiraPolicy jiraPolicy, Equilobe.DailyReport.Models.Web.ReportSettingsSummary policySummary, Equilobe.DailyReport.Models.Web.ReportPolicy policyDetails);
        string GetJiraUsername(System.Collections.Specialized.NameValueCollection queryString);
        string GetJiraBaseUrl(System.Collections.Specialized.NameValueCollection queryString);
    }
}
