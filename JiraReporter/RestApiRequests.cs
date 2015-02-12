using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class RestApiRequests
    {
        //public static RestClient ClientLogin(JiraPolicy policy)
        //{
        //    var client = new RestClient(policy.BaseUrl);

        //    if (!String.IsNullOrEmpty(policy.SharedSecret))
        //        client.Authenticator = new JwtAuthenticator(policy.SharedSecret);
        //    else
        //        client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);

        //    return client;
        //}

        //public static T ResolveRequest<T>(JiraPolicy policy, RestRequest request, bool isXml = false)
        //{
        //    var client = ClientLogin(policy);
        //    var response = client.Execute(request);

        //    ValidateResponse(response);

        //    if (isXml)
        //        return Deserialization.XmlDeserialize<T>(response.Content);
        //    else
        //        return Deserialization.JsonDeserialize<T>(response.Content);
        //}

        //public static T ResolveJiraRequest<T>(JiraPolicy policy, RestRequest request) where T : new()
        //{
        //    var client = ClientLogin(policy);
        //    var response = client.Execute<T>(request);

        //    ValidateResponse(response);

        //    return response.Data;
        //}

        //private static void ValidateResponse(IRestResponse response)
        //{
        //    if (response.ResponseStatus != ResponseStatus.Completed || response.ErrorException != null || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NoContent)
        //        throw new InvalidOperationException(string.Format("RestSharp status: {0}, HTTP response: {1}", response.ResponseStatus, !String.IsNullOrEmpty(response.ErrorMessage) ? response.ErrorMessage : response.StatusDescription));
        //}

        //public static Project GetProject(JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.Project(policy.ProjectId.ToString()), Method.GET);

        //    return ResolveRequest<Project>(policy, request);
        //}

        //public static Timesheet GetTimesheetForUser(JiraPolicy policy, DateTime startDate, DateTime endDate, string targetUser)
        //{
        //    var request = new RestRequest(JiraApiUrls.TimesheetForUser(TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate), targetUser), Method.GET);

        //    return ResolveRequest<Timesheet>(policy, request, true);
        //}

        //public static Timesheet GetTimesheet(JiraPolicy policy, DateTime startDate, DateTime endDate)
        //{
        //    var request = new RestRequest(JiraApiUrls.Timesheet(TimeFormatting.DateToString(startDate), TimeFormatting.DateToString(endDate)), Method.GET);

        //    return ResolveRequest<Timesheet>(policy, request, true);
        //}

        //public static JiraUser GetUser(string username, JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.User(username), Method.GET);

        //    return ResolveRequest<JiraUser>(policy, request);
        //}

        //public static List<JiraUser> GetUsers(JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.Users(policy.GeneratedProperties.ProjectKey), Method.GET);

        //    return ResolveRequest<List<JiraUser>>(policy, request);
        //}

        //public static RapidView GetRapidView(string id, JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.RapidView(id), Method.GET);

        //    return ResolveRequest<RapidView>(policy, request);
        //}

        //public static List<View> GetRapidViews(JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.RapidViews(), Method.GET);

        //    return ResolveRequest<Views>(policy, request).views;
        //}

        //public static SprintReport GetSprintReport(string rapidViewId, string sprintId, JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.Sprint(rapidViewId, sprintId), Method.GET);

        //    return ResolveRequest<SprintReport>(policy, request);
        //}

        //public static List<Sprint> GetAllSprints(string rapidViewId, JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.AllSprints(rapidViewId), Method.GET);

        //    return ResolveRequest<Sprints>(policy, request).sprints;
        //}

        //public static JiraIssue GetIssue(string issueKey, JiraPolicy policy)
        //{
        //    var request = new RestRequest(JiraApiUrls.Issue(issueKey), Method.GET);

        //    return ResolveJiraRequest<JiraIssue>(policy, request);
        //}

        //public static JiraIssues GetCompletedIssues(JiraPolicy policy, DateTime startDate, DateTime endDate)
        //{
        //    var request = GetIssuesByJql(JiraApiUrls.ResolvedIssues(TimeFormatting.DateToISO(startDate), TimeFormatting.DateToISO(endDate)));

        //    return ResolveJiraRequest<JiraIssues>(policy, request);
        //}

        //public static JiraIssues GetSprintTasks(JiraPolicy policy)
        //{
        //    var request = GetIssuesByJql(JiraApiUrls.IssuesInOpenSprints(policy.GeneratedProperties.ProjectKey));

        //    return ResolveJiraRequest<JiraIssues>(policy, request);
        //}

        //public static RestRequest GetIssuesByJql(string jql)
        //{
        //    return new RestRequest(JiraApiUrls.Search(jql), Method.GET);
        //}
    }
}
