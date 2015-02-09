using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Atlassian.Connect;
using RestSharp;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter;
using DailyReportWeb.Services;
using JiraReporter.Services;

namespace DailyReportWeb.Controllers.Api
{
 //   [Authorize]
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
            return new ReportSettings();
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
            var policy = new JiraPolicy
            {
                BaseUrl = policySummary.BaseUrl,
                SharedSecret = policySummary.SharedSecret,
                ProjectId = Int32.Parse(policySummary.Id),
                ReportTime = policySummary.Time
            };

            JiraPolicyService.SaveToFile(@"C:\Workspace\DailyReportTool\JiraReporter\Policies\testing.txt", policy);
        }

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
