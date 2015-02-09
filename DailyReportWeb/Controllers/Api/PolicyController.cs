using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DailyReportWeb.Models;
using SourceControlLogReporter.Model;
using System.Web;
using Atlassian.Connect;
using RestSharp;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Storage;

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

            return PolicySummary.GetPoliciesSummary(baseUrl, sharedSecret);
        }

        // GET: api/Policy/5
        public ReportSettings Get(long id)
        {
            using (var db = new ReportsDb())
            {
                var baseUrl = User.GetBaseUrl();
                return db.ReportSettings.Single(qr => qr.ProjectId == id && qr.BaseUrl == baseUrl);
            }
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Policy/5
        public void Put(long id, [FromBody]Policy updatedPolicy)
        {
            // TODO: save the updated policy
        }

        public void Put([FromBody]PolicySummary policySummary)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.Single(qr => qr.ProjectId == policySummary.ProjectId && qr.BaseUrl == policySummary.BaseUrl);

                if (reportSettings == null)
                {
                    reportSettings = CreateFromPolicySummary(policySummary);
                    db.ReportSettings.Add(reportSettings);
                }

                reportSettings.ReportTime = policySummary.ReportTime;

                db.SaveChanges();
            }
        }

        public static ReportSettings CreateFromPolicySummary(PolicySummary policySummary)
        {
            return new ReportSettings
            {
                BaseUrl = policySummary.BaseUrl,
                SharedSecret = policySummary.SharedSecret,
                ProjectId = policySummary.ProjectId
            };
        }

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
