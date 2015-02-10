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
            using (var db = new ReportsDb())
            {
                var baseUrl = User.GetBaseUrl();
                return db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == id && qr.BaseUrl == baseUrl);
        }
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Policy/5
        public void Put(string id, [FromBody]JiraPolicy updatedPolicy)
        {
            // TODO: save the updated policy
        }

        public void Put([FromBody]PolicySummary policySummary)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == policySummary.ProjectId && qr.BaseUrl == policySummary.BaseUrl);

                if (reportSettings == null)
                {
                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);

                    reportSettings.BaseUrl = policySummary.BaseUrl;
                    reportSettings.SharedSecret = policySummary.SharedSecret;
                    reportSettings.ProjectId = policySummary.ProjectId;
        }

                reportSettings.ReportTime = policySummary.ReportTime;

                db.SaveChanges();
            }
        }

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
