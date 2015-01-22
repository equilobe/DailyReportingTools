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

            if (!String.IsNullOrEmpty(policy.SharedSecret))
                client.Authenticator = new JwtAuthenticator(policy.SharedSecret);
            else
                client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);

            var results = client.Execute(request).Content;

            if (isXml)
                return Deserialization.XmlDeserialize<T>(results);
            else
                return Deserialization.JsonDeserialize<T>(results);
        }

        public static JiraReporter.JiraModels.Project GetProject(Policy policy)
        {
            var request = new RestRequest(ApiUrls.Project(policy.ProjectId.ToString()), Method.GET);

            return ResolveRequest<JiraReporter.JiraModels.Project>(policy, request);
        }

        public static Timesheet GetTimesheet(Policy policy, DateTime startDate, DateTime endDate, string targetUser)
        {
            var request = new RestRequest(ApiUrls.Timesheet(targetUser, TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate)), Method.GET);

            return ResolveRequest<Timesheet>(policy, request, true);
        }

        public static List<JiraUser> GetUsers(Policy policy)
        {
            var request = new RestRequest(ApiUrls.Users(policy.GeneratedProperties.ProjectKey), Method.GET);

            return ResolveRequest<List<JiraUser>>(policy, request);
        }

        public static RapidView GetRapidView(string id, Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidView(id), Method.GET);
            
            return ResolveRequest<RapidView>(policy, request);
        }
       
        public static List<View> GetRapidViews(Policy policy)
        {
            var request = new RestRequest(ApiUrls.RapidViews(), Method.GET);

            return ResolveRequest<Views>(policy, request).views;
        }

        public static SprintReport GetSprintReport(string rapidViewId, string sprintId, Policy policy)
        {
            var request = new RestRequest(ApiUrls.Sprint(rapidViewId, sprintId), Method.GET);
            
            return ResolveRequest<SprintReport>(policy, request);
        }

        public static Issues GetCompletedIssues(Policy policy, DateTime startDate, DateTime endDate)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));

            return client.GetIssuesByJql(ApiUrls.ResolvedIssues(TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)), 0, 250);
        }

        public static Issues GetSprintTasks(Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));

            return client.GetIssuesByJql(ApiUrls.IssuesInOpenSprints(policy.GeneratedProperties.ProjectKey), 0, 250);
        }

        public static AnotherJiraRestClient.Issue GetIssue(string issueKey, Policy policy)
        {
            var client = new JiraClient(new JiraAccount(policy.BaseUrl, policy.Username, policy.Password));

            return client.GetIssue(issueKey);
        }
    }
}
