using Equilobe.DailyReport.JWT;
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

        public static JiraClient CreateWithBasicAuth(string baseUrl, string username, string password)
        {
            return new JiraClient
            {
                Client = RestApiHelper.BasicAuthentication(baseUrl, username, password)
            };
        }

        public static JiraClient CreateWithJwt(string baseUrl, string sharedSecret, string addonKey)
        {
            return new JiraClient
            {
                Client =  RestApiHelper.JwtAuthentication(baseUrl, sharedSecret, addonKey)
            };
        }


        protected JiraClient()
        {
        }

        T ResolveRequest<T>(RestRequest request, bool isXml = false) where T : new()
        {
            return RestApiHelper.ResolveRequest<T>(Client, request, isXml);
        }

        T ResolveJiraRequest<T>(RestRequest request) where T : new()
        {
            return RestApiHelper.ResolveJiraRequest<T>(Client, request);
        }

        static void ValidateResponse(IRestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NoContent ||
                response.ErrorException != null ||
                response.ResponseStatus != ResponseStatus.Completed)
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

        public JiraIssue GetIssue(string issueKey)
        {
            var request = new RestRequest(JiraApiUrls.Issue(issueKey), Method.GET);

            return ResolveJiraRequest<JiraIssue>(request);
        }

        public Worklogs GetIssueWorklogs(string issueKey)
        {
            var request = new RestRequest(JiraApiUrls.IssueWorklogs(issueKey), Method.GET);

            return ResolveJiraRequest<Worklogs>(request);
        }

        public JiraIssues GetCompletedIssues(string projectKey, DateTime startDate, DateTime endDate)
        {
            var request = GetIssuesByJql(JiraApiUrls.ResolvedIssues(projectKey, TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)));

            return ResolveJiraRequest<JiraIssues>(request);
        }

        public JiraIssues GetSprintTasks(string projectKey, string sprintId)
        {
            var request = GetIssuesByJql(JiraApiUrls.IssueInCurrentSprint(projectKey, sprintId));

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

        public Board Board(string projectKey)
        {
            var request = new RestRequest(JiraApiUrls.Board(projectKey), Method.GET);

            return ResolveJiraRequest<JiraBoard>(request).Values[0];
        }

        public JiraSprints GetAllSprints(string boardId, string startAt)
        {
            var request = new RestRequest(JiraApiUrls.AllSprints(boardId, startAt), Method.GET);

            return ResolveJiraRequest<JiraSprints>(request);
        }
    }
}
