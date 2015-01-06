using AnotherJiraRestClient;
using JiraReporter.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class RestApiRequests
    {
        public static string ResolveRequest(SourceControlLogReporter.Model.Policy policy, RestRequest request)
        {
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);

            return client.Execute(request).Content;
        }

        public static Timesheet GetTimesheet(SourceControlLogReporter.Model.Policy policy, DateTime startDate, DateTime endDate)
        {
            var request = new RestRequest(ApiUrls.Timesheet(policy.TargetGroup, TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate)), Method.GET);
            var xmlString = ResolveRequest(policy, request);

            return Deserialization.XmlDeserialize<Timesheet>(xmlString);
        }

        public static List<User> GetUsers(SourceControlLogReporter.Model.Policy policy)
        {
            var request = new RestRequest(ApiUrls.Users(policy.ProjectKey), Method.GET);
            var contentString = ResolveRequest(policy, request);

            return Deserialization.JsonDeserialize<List<User>>(contentString);
        }

        public static RapidView GetRapidView(string id, SourceControlLogReporter.Model.Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidView(id), Method.GET);
            var contentString = ResolveRequest(policy, request);

            return Deserialization.JsonDeserialize<RapidView>(contentString);
        }
       
        public static List<View> GetRapidViews(SourceControlLogReporter.Model.Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidViews(), Method.GET);
            var contentString = ResolveRequest(policy, request);

            return Deserialization.JsonDeserialize<Views>(contentString).views;
        }

        public static SprintReport GetSprintReport(string rapidViewId, string sprintId, SourceControlLogReporter.Model.Policy policy)
        {
            var request = new RestRequest(ApiUrls.Sprint(rapidViewId, sprintId), Method.GET);
            var contentString = ResolveRequest(policy, request);

            return Deserialization.JsonDeserialize<SprintReport>(contentString);
        }

        public static AnotherJiraRestClient.Issues GetCompletedIssues(SourceControlLogReporter.Model.Policy policy, DateTime startDate, DateTime endDate)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var issues = client.GetIssuesByJql(ApiUrls.ResolvedIssues(TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)), 0, 250);

            return issues;
        }

        public static AnotherJiraRestClient.Issues GetSprintTasks(SourceControlLogReporter.Model.Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var tasks = client.GetIssuesByJql(ApiUrls.IssuesInOpenSprints(policy.ProjectKey), 0, 250);

            return tasks;
        }

        public static AnotherJiraRestClient.Issue GetIssue(string issueKey, SourceControlLogReporter.Model.Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));
            var issue = client.GetIssue(issueKey);

            return client.GetIssue(issueKey);
        }
    }
}
