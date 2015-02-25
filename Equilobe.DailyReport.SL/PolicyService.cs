using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System.Collections.Specialized;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class PolicyService
    {
        public static string GetJiraBaseUrl(NameValueCollection queryString)
        {
            var baserUrl = queryString["xdm_e"] + queryString["cp"];
            return baserUrl;
        }

        public static string GetJiraUsername(NameValueCollection queryString)
        {
            var userId = queryString["user_id"];
            return userId;
        }

        public static PolicyBuffer GetSyncedPolicy(string baseUrl, string sharedSecret, long projectId)
        {
            var jiraPolicy = GetPolicyFromJira(baseUrl, sharedSecret, projectId);
            var policySummary = PolicySummaryService.GetPolicySummary(baseUrl, sharedSecret, projectId);
            var policy = GetPolicyFromDb(baseUrl, projectId);

            return SyncPolicy(jiraPolicy, policySummary, policy);
        }

        public static JiraPolicy GetPolicyFromJira(string baseUrl, string sharedSecret, long projectId)
        {
            var context = new JiraRequestContext
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret
            };

            var policy = new JiraPolicy
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret,
                ProjectId = projectId
            };

            var project = new JiraService().GetProject(context, projectId);
            policy.UserOptions = new JiraService().GetUsers(context, project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return policy;
        }

        public static PolicyDetails GetPolicyFromDb(string baseUrl, long projectId)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl);

                if (reportSettings != null && reportSettings.SerializedPolicy != null)
                    return Deserialization.XmlDeserialize<PolicyDetails>(reportSettings.SerializedPolicy.PolicyString);

                return null;
            }
        }

        public static PolicyBuffer GetPolicyBufferFromDb(string uniqueProjectKey)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == uniqueProjectKey);

                if (reportSettings == null || reportSettings.SerializedPolicy == null)
                    return null;

                var policyBuffer = new PolicyBuffer();
                reportSettings.CopyProperties<IReportSetting>(policyBuffer);

                if (!string.IsNullOrEmpty(reportSettings.SerializedPolicy.PolicyString))
                    Deserialization.XmlDeserialize<PolicyDetails>(reportSettings.SerializedPolicy.PolicyString)
                        .CopyProperties(policyBuffer);

                return policyBuffer;
            }
        }

        public static PolicyBuffer SyncPolicy(JiraPolicy jiraPolicy, PolicySummary policySummary, PolicyDetails policy)
        {
            var policyBuffer = new PolicyBuffer();
            jiraPolicy.CopyProperties(policyBuffer);

            policySummary.CopyProperties(policyBuffer);

            policy.CopyProperties(policyBuffer);

            return policyBuffer;
        }
    }
}
