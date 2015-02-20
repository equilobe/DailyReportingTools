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
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using JiraReporter;
using DailyReportWeb.Services;
using JiraReporter.Services;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.ReportFrame;
using System.IO;
using System.Data.Entity;

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
                    reportSettings.UniqueProjectKey = policySummary.ProjectKey + Path.GetRandomFileName().Replace(".", string.Empty);
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
                    reportSettings.UniqueProjectKey = updatedReportSettings.Policy.GeneratedProperties.ProjectKey + Path.GetRandomFileName().Replace(".", string.Empty);
                }
                updatedReportSettings.CopyProperties(reportSettings);
                reportSettings.SerializedPolicy.PolicyString = Serialization.XmlSerialize(updatedReportSettings.Policy);

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
                ProjectId = policySummary.ProjectId
            };
        }

        private ReportSettings SyncReportSettingsToJira(long id, string baseUrl, string sharedSecret)
        {
            var jiraPolicy = GetReportPolicyFromJira(id, baseUrl, sharedSecret);
            var reportSettings = GetReportPolicyFromDb(id, baseUrl, sharedSecret);
          
            //reportSettings.Policy = (reportSettings.SerializedPolicy == null || (reportSettings.SerializedPolicy != null && reportSettings.SerializedPolicy.PolicyString == null)) ? jiraPolicy : SyncPolicyToJira(jiraPolicy, reportSettings.SerializedPolicy.PolicyString);
            //using (var context = new ReportsDb())
            //{

 
            //}

            if (reportSettings.SerializedPolicy != null)
            {
                if (reportSettings.SerializedPolicy.PolicyString != null)
                {
                    reportSettings.Policy = SyncPolicyToJira(jiraPolicy, reportSettings.SerializedPolicy.PolicyString);
                }
            }
            else reportSettings.Policy = jiraPolicy;

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
                        ProjectId = id
                    };
                }

                return reportSettings;
            }
        }

        private static JiraPolicy GetReportPolicyFromJira(long id, string baseUrl, string sharedSecret)
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
                ProjectId = id
            };

            var project = new JiraService().GetProject(context, id.ToString());
            policy.GeneratedProperties = new JiraGeneratedProperties
            {
                ProjectName = project.Name,
                ProjectKey = project.Key
            };

            policy.UserOptions = new JiraService().GetUsers(context, project.Key)
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
