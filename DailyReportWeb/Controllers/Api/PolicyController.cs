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
                return db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == id && qr.BaseUrl == baseUrl);
            }
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // PUT: api/Policy/5
        public void Put(string id, [FromBody]Policy updatedPolicy)
        {
            // TODO: save the updated policy
        }

        public void Put([FromBody]PolicySummary policySummary)
        {
            using (var db = new ReportsDb())
            {
                db.ReportSettings.Add(new ReportSettings
                {
                    BaseUrl = policySummary.BaseUrl,
                    SharedSecret = policySummary.SharedSecret,
                    ProjectId = policySummary.ProjectId,
                    ReportTime = policySummary.ReportTime,
                    PolicyXml = JiraReporter.Serialization.XmlSerialize(new Policy
                    {
                        BaseUrl = policySummary.BaseUrl,
                        SharedSecret = policySummary.SharedSecret,
                        ProjectId = policySummary.ProjectId,
                        ReportTime = policySummary.ReportTime,
                    })
                });

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
