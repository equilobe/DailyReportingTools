﻿using System;
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

namespace DailyReportWeb.Controllers.Api
{
    public class PolicyController : ApiController
    {
        // GET: api/Policy
        public IEnumerable<PolicySummary> Get()
        {
			var urlQs = HttpUtility.ParseQueryString(Request.Headers.Referrer.Query);
			var baseUrl = urlQs["xdm_e"] + urlQs["cp"];

			var sharedSecret = SecretKeyProviderFactory.GetSecretKeyProvider().GetSecretKey(baseUrl);

			var policy = new Policy();
			policy.BaseUrl = baseUrl;
			policy.SharedSecret = sharedSecret;
			
			var request = new RestRequest(JiraReporter.ApiUrls.Project(), Method.GET);

			var policies = JiraReporter.RestApiRequests.ResolveRequest<List<PolicySummary>>(policy, request);
			return policies;
        }

        // GET: api/Policy/5
        public Policy Get(string id)
        {
            return new Policy();
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

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
