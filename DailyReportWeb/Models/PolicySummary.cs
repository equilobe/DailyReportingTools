using RestSharp;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DailyReportWeb.Models
{
    [DataContract]
    public class ProjectInfo
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    public class PolicySummary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public string BaseUrl { get; set; }
        public string SharedSecret { get; set; }

        public static List<PolicySummary> GetPoliciesSummary(string baseUrl, string sharedSecret)
        {
            return GetProjectsInfo(baseUrl, sharedSecret)
                .Select(projectInfo => new PolicySummary
                {
                    BaseUrl = baseUrl,
                    SharedSecret = sharedSecret,
                    Id = projectInfo.Id,
                    Name = projectInfo.Name
                })
                .ToList();
        }

        private static List<ProjectInfo> GetProjectsInfo(string baseUrl, string sharedSecret)
        {
            var policy = new Policy
            {
                BaseUrl = baseUrl,
                SharedSecret = sharedSecret
            };

            var request = new RestRequest(JiraReporter.ApiUrls.Project(), Method.GET);

            return JiraReporter.RestApiRequests.ResolveRequest<List<ProjectInfo>>(policy, request);
        }
    }
}