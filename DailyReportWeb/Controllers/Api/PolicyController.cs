using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Atlassian.Connect;
using RestSharp;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportPolicy;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter;
using DailyReportWeb.Services;
using JiraReporter.Services;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class PolicyController : ApiController
    {
        // GET: api/Policy
        public IEnumerable<PolicySummary> Get()
        {
            var baseUrl = User.GetBaseUrl();
            var sharedSecret = SecretKeyProviderFactory.GetSecretKeyProvider().GetSecretKey(baseUrl);

            return PolicySummaryService.GetPoliciesSummary(baseUrl, sharedSecret);
        }

        // GET: api/Policy/5
        public ReportSettings Get(long id)
        {
            var baseUrl = User.GetBaseUrl();
            var sharedSecret = SecretKeyProviderFactory.GetSecretKeyProvider().GetSecretKey(baseUrl);

            return SyncReportSettingsToJira(id, baseUrl, sharedSecret);
        }

        public void Put([FromBody]PolicySummary policySummary)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == policySummary.ProjectId && qr.BaseUrl == policySummary.BaseUrl);
                if (reportSettings == null)
                {
                    reportSettings = CreateFromPolicySummary(policySummary);
                    db.ReportSettings.Add(reportSettings);
                }
                reportSettings.ReportTime = policySummary.ReportTime;

                db.SaveChanges();
            }
        }

        // PUT: api/Policy/5
        public void Put(long id, [FromBody]ReportSettings updatedReportSettings)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == updatedReportSettings.ProjectId && qr.BaseUrl == updatedReportSettings.BaseUrl);
                if (reportSettings == null)
                {
                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);
                }
                updatedReportSettings.CopyProperties(reportSettings);
                reportSettings.PolicyXml = Serialization.XmlSerialize(updatedReportSettings.Policy);

                db.SaveChanges();
            }
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}

        private static ReportSettings CreateFromPolicySummary(PolicySummary policySummary)
        {
            return new ReportSettings
            {
                BaseUrl = policySummary.BaseUrl,
                SharedSecret = policySummary.SharedSecret,
                ProjectId = policySummary.ProjectId
            };
        }

        private ReportSettings SyncReportSettingsToJira(long id, string baseUrl, string sharedSecret)
        {
            var jiraPolicy = GetReportPolicyFromJira(id, baseUrl, sharedSecret);
            var reportSettings = GetReportPolicyFromDb(id, baseUrl, sharedSecret);
            reportSettings.Policy = reportSettings.PolicyXml == null ? jiraPolicy : SyncPolicyToJira(jiraPolicy, reportSettings.PolicyXml);

            return reportSettings;
        }

        private JiraPolicy SyncPolicyToJira(JiraPolicy policy, string policyXml)
        {
            Deserialization.XmlDeserialize<JiraPolicy>(policyXml)
                .CopyProperties(policy);

            return policy;
        }

        private static ReportSettings GetReportPolicyFromDb(long id, string baseUrl, string sharedSecret)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == id && qr.BaseUrl == baseUrl);
                if (reportSettings == null)
                {
                    reportSettings = new ReportSettings
                    {
                        BaseUrl = baseUrl,
                        SharedSecret = sharedSecret,
                        ProjectId = id
                    };
                }

                return reportSettings;
            }
        }

        private static JiraPolicy GetReportPolicyFromJira(long id, string baseUrl, string sharedSecret)
        {
            var policy = new JiraPolicy
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret,
                ProjectId = id
            };

            var project = RestApiRequests.GetProject(policy);
            policy.GeneratedProperties = new JiraGeneratedProperties
            {
                ProjectName = project.Name,
                ProjectKey = project.Key
            };

            policy.UserOptions = RestApiRequests.GetUsers(policy)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return policy;
        }
    }
}
