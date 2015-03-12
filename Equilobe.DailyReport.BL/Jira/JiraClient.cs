using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Utils;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Equilobe.DailyReport.BL.Jira
{
    public class JiraClient
    {
        RestClient Client { get; set; }

        public JiraClient(string baseUrl, string username, string password)
        {
            Client = new RestClient(baseUrl);
            Client.Authenticator = new HttpBasicAuthenticator(username, password);
        }

        public JiraClient(string baseUrl, string sharedSecret)
        {
            Client = new RestClient(baseUrl);
            Client.Authenticator = new JwtAuthenticator(sharedSecret);
        }

        T ResolveRequest<T>(RestRequest request, bool isXml = false)
        {
            var response = Client.Execute(request);

            ValidateResponse(response);

            if (isXml)
                return Deserialization.XmlDeserialize<T>(response.Content);
            else
                return Deserialization.JsonDeserialize<T>(response.Content);
        }

        T ResolveJiraRequest<T>(RestRequest request) where T : new()
        {
            var response = Client.Execute<T>(request);

            ValidateResponse(response);

            return response.Data;
        }

        static void ValidateResponse(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.ErrorException != null || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NoContent)
                throw new InvalidOperationException(string.Format("RestSharp status: {0}, HTTP response: {1}", response.ResponseStatus, !String.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : response.StatusDescription));
        }

        public Project GetProject(long id)
        {
            var request = new RestRequest(JiraApiUrls.Project(id), Method.GET);

            return ResolveRequest<Project>(request);
        }

        public JiraUser GetUser(string username)
        {
            var request = new RestRequest(JiraApiUrls.User(username), Method.GET);

            return ResolveRequest<JiraUser>(request);
        }

        public List<JiraUser> GetUsers(string projectKey)
        {
            var request = new RestRequest(JiraApiUrls.Users(projectKey), Method.GET);

            return ResolveRequest<List<JiraUser>>(request)
                .Where(user => !user.key.StartsWith("addon_"))
                .ToList();
        }

        public RapidView GetRapidView(string id)
        {
            var request = new RestRequest(JiraApiUrls.RapidView(id), Method.GET);

            return ResolveRequest<RapidView>(request);
        }

        public List<View> GetRapidViews()
        {
            var request = new RestRequest(JiraApiUrls.RapidViews(), Method.GET);

            return ResolveRequest<Views>(request).views;
        }

        public SprintReport GetSprintReport(string rapidViewId, string sprintId)
        {
            var request = new RestRequest(JiraApiUrls.Sprint(rapidViewId, sprintId), Method.GET);

            return ResolveRequest<SprintReport>(request);
        }

        public List<Sprint> GetAllSprints(string rapidViewId)
        {
            var request = new RestRequest(JiraApiUrls.AllSprints(rapidViewId), Method.GET);

            return ResolveRequest<Sprints>(request).sprints;
        }

        public JiraIssue GetIssue(string issueKey)
        {
            var request = new RestRequest(JiraApiUrls.Issue(issueKey), Method.GET);

            return ResolveJiraRequest<JiraIssue>(request);
        }

        public JiraIssues GetCompletedIssues(string projectKey, DateTime startDate, DateTime endDate)
        {
            var request = GetIssuesByJql(JiraApiUrls.ResolvedIssues(projectKey, TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)));

            return ResolveJiraRequest<JiraIssues>(request);
        }

        public JiraIssues GetSprintTasks(string projectKey)
        {
            var request = GetIssuesByJql(JiraApiUrls.IssuesInOpenSprints(projectKey));

            return ResolveJiraRequest<JiraIssues>(request);
        }

        public ProjectInfo GetProjectInfo(long id)
        {
            var request = new RestRequest(JiraApiUrls.Project(id), Method.GET);

            return ResolveRequest<ProjectInfo>(request);
        }

        public List<ProjectInfo> GetProjectsInfo()
        {
            var request = new RestRequest(JiraApiUrls.Projects(), Method.GET);

            return ResolveRequest<List<ProjectInfo>>(request);
        }

        public RestRequest GetIssuesByJql(string jql)
        {
            return new RestRequest(JiraApiUrls.Search(jql), Method.GET);
        }

        public List<JiraBasicIssue> GetWorklogs(string projectKey, string author, string fromDate, string toDate)
        {
            var request = GetIssuesByJql(JiraApiUrls.WorkLogs(projectKey, author, fromDate, toDate));

            return ResolveRequest<JiraBasicIssues>(request).issues;
        }
    }
}
