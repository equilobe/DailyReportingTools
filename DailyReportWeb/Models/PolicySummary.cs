using RestSharp;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Models
{
    public class PolicySummary
    {
        public string id { get; set; }
        public string name { get; set; }
        public string time { get; set; }
        public string BaseUrl { get; set; }
        public string SharedSecret { get; set; }

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            var policy = new Policy
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret
            };

            var request = new RestRequest(JiraReporter.ApiUrls.Project(), Method.GET);

            var policiesSumary = JiraReporter.RestApiRequests.ResolveRequest<List<PolicySummary>>(policy, request);
            foreach (PolicySummary policySumary in policiesSumary)
            {
                policySumary.BaseUrl = baseUrl;
                policySumary.SharedSecret = sharedSecret;
            }

            return policiesSumary;
        }
    }
}