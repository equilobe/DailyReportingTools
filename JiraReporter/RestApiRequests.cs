using JiraReporter.JiraModels;
using JiraReporter.JiraModels;
using JiraReporter.Model;
using RestSharp;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class RestApiRequests
    {
        public static RestClient ClientLogin(Policy policy)
        {
            var client = new RestClient(policy.BaseUrl);

            if (!String.IsNullOrEmpty(policy.SharedSecret))
                client.Authenticator = new JwtAuthenticator(policy.SharedSecret);
            else
                client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);

            return client;
        }

        public static T ResolveRequest<T>(Policy policy, RestRequest request, bool isXml = false)
        {
            var client = ClientLogin(policy);
            var response = client.Execute(request);

            ValidateResponse(response);

            if (isXml)
                return Deserialization.XmlDeserialize<T>(response.Content);
            else
                return Deserialization.JsonDeserialize<T>(response.Content);
        }

        public static T ResolveJiraRequest<T>(Policy policy, RestRequest request) where T : new()
        {
            var client = ClientLogin(policy);
            var response = client.Execute<T>(request);

            ValidateResponse(response);

            return response.Data;
        }

        private static void ValidateResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.ErrorException != null || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NoContent)
                throw new InvalidOperationException(string.Format("RestSharp status: {0}, HTTP response: {1}", response.ResponseStatus, !String.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : response.StatusDescription));
        }

        public static JiraModels.Project GetProject(Policy policy)
        {
            var request = new RestRequest(ApiUrls.Project(policy.ProjectId.ToString()), Method.GET);

            return ResolveRequest<JiraModels.Project>(policy, request);
        }

        public static Timesheet GetTimesheet(Policy policy, DateTime startDate, DateTime endDate, string targetUser)
        {
            var request = new RestRequest(ApiUrls.Timesheet(targetUser, TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate)), Method.GET);

            return ResolveRequest<Timesheet>(policy, request, true);
        }

        public static JiraUser GetUser(string username, Policy policy)
        {
            var request = new RestRequest(ApiUrls.User(username), Method.GET);

            return ResolveRequest<JiraUser>(policy, request);
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

        public static List<Sprint> GetAllSprints(string rapidViewId, Policy policy)
        {
            var request = new RestRequest(ApiUrls.AllSprints(rapidViewId), Method.GET);

            return ResolveRequest<Sprints>(policy, request).sprints;
        }

        public static JiraIssue GetIssue(string issueKey, Policy policy)        
        {
            var request = new RestRequest(ApiUrls.IssueByKey(issueKey), Method.GET);

            return ResolveJiraRequest<JiraIssue>(policy, request);
        }

        public static JiraIssues GetCompletedIssues(Policy policy, DateTime startDate, DateTime endDate)
        {
            var request = GetIssuesByJql(ApiUrls.ResolvedIssues(TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)));

            return ResolveJiraRequest<JiraIssues>(policy, request);
        }

        public static JiraIssues GetSprintTasks(Policy policy)
        {
            var request = GetIssuesByJql(ApiUrls.IssuesInOpenSprints(policy.GeneratedProperties.ProjectKey));

            return ResolveJiraRequest<JiraIssues>(policy, request);
        }

        public static RestRequest GetIssuesByJql(string jql)
        {
            return new RestRequest(ApiUrls.Search(jql), Method.GET);
        }
    }
}
