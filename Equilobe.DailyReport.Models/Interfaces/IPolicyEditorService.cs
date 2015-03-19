using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Web;
using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IPolicyEditorService : IService
    {
        JiraPolicy CreateNewJiraPolicyForProject(long projectId);
        PolicyBuffer GetPolicy(long projectId);
        PolicyDetails GetPolicyDetails(string baseUrl, long projectId);
        PolicyBuffer SyncPolicy(Equilobe.DailyReport.Models.Policy.JiraPolicy jiraPolicy, Equilobe.DailyReport.Models.Web.PolicySummary policySummary, Equilobe.DailyReport.Models.Web.PolicyDetails policyDetails);
        string GetJiraUsername(System.Collections.Specialized.NameValueCollection queryString);
        string GetJiraBaseUrl(System.Collections.Specialized.NameValueCollection queryString);
    }
}
