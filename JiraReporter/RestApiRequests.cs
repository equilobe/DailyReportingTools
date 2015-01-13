using AnotherJiraRestClient;
using JiraReporter.Model;
using RestSharp;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class RestApiRequests
    {
        public static T ResolveRequest<T>(Policy policy, RestRequest request, bool isXml = false)
        {
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);

            var results = client.Execute(request).Content;

            if (isXml)
                return Deserialization.XmlDeserialize<T>(results);
            else
                return Deserialization.JsonDeserialize<T>(results);
        }

        public static T ResolveRequestJwt<T>(Policy policy, RestRequest request, bool isXml = false)
        {
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new JwtAuthenticator(policy.SharedSecret);

            var results = client.Execute(request).Content;

            if (isXml)
                return Deserialization.XmlDeserialize<T>(results);
            else
                return Deserialization.JsonDeserialize<T>(results);
        }

        public static Timesheet GetTimesheet(Policy policy, DateTime startDate, DateTime endDate)
        {
            var request = new RestRequest(ApiUrls.Timesheet(policy.TargetGroup, TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate)), Method.GET);
            var xmlString = ResolveRequest<Timesheet>(policy, request, true);

            return xmlString;
        }

        public static List<JiraUser> GetUsers(Policy policy)
        {
            var request = new RestRequest(ApiUrls.Users(policy.ProjectKey), Method.GET);
            var contentString = ResolveRequest<List<JiraUser>>(policy, request);

            return contentString;
        }

        public static RapidView GetRapidView(string id, Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidView(id), Method.GET);
            var contentString = ResolveRequest<RapidView>(policy, request);

            return contentString;
        }
       
        public static List<View> GetRapidViews(Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidViews(), Method.GET);
            var contentString = ResolveRequest<Views>(policy, request);

            return contentString.views;
        }

        public static SprintReport GetSprintReport(string rapidViewId, string sprintId, Policy policy)
        {
            var request = new RestRequest(ApiUrls.Sprint(rapidViewId, sprintId), Method.GET);
            var contentString = ResolveRequest<SprintReport>(policy, request);

            return contentString;
        }

        public static Issues GetCompletedIssues(Policy policy, DateTime startDate, DateTime endDate)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var issues = client.GetIssuesByJql(ApiUrls.ResolvedIssues(TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)), 0, 250);

            return issues;
        }

        public static Issues GetSprintTasks(Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var tasks = client.GetIssuesByJql(ApiUrls.IssuesInOpenSprints(policy.ProjectKey), 0, 250);

            return tasks;
        }

        public static AnotherJiraRestClient.Issue GetIssue(string issueKey, Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var issue = client.GetIssue(issueKey);

            return issue;
        }
    }
}
