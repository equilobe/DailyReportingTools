using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicyService
    {
        string GetJiraBaseUrl(System.Collections.Specialized.NameValueCollection queryString);
        Equilobe.DailyReport.Models.Policy.JiraPolicy GetJiraPolicy(long projectId);
        string GetJiraUsername(System.Collections.Specialized.NameValueCollection queryString);
        Equilobe.DailyReport.Models.Web.PolicyBuffer GetPolicy(long projectId);
        Equilobe.DailyReport.Models.Policy.JiraPolicy GetPolicy(string uniqueProjectKey);
        Equilobe.DailyReport.Models.Web.PolicyBuffer GetPolicyBufferFromDb(string uniqueProjectKey);
        Equilobe.DailyReport.Models.Web.PolicyDetails GetPolicyDetails(string baseUrl, long projectId);
        Equilobe.DailyReport.Models.Interfaces.IJiraRequestContextService JiraRequestContextService { get; set; }
        Equilobe.DailyReport.Models.Web.PolicyBuffer SyncPolicy(Equilobe.DailyReport.Models.Policy.JiraPolicy jiraPolicy, Equilobe.DailyReport.Models.Web.PolicySummary policySummary, Equilobe.DailyReport.Models.Web.PolicyDetails policyDetails);
    }
}
