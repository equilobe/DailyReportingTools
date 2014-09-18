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
        public static Timesheet GetTimesheet(Policy policy, DateTime startDate, DateTime endDate)
        {
            string fromDate = Options.DateToString(startDate);
            string toDate = Options.DateToString(endDate);
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);
            var request = new RestRequest(ApiUrls.Timesheet(policy.TargetGroup, fromDate, toDate), Method.GET);
            var response = client.Execute(request);
            string xmlString = response.Content;
            return Deserialization.XmlDeserialize<Timesheet>(xmlString);
        }

        public static List<User> GetUsers(Policy policy)
        {
            var client = new RestClient(policy.BaseUrl);
            client.Authenticator = new HttpBasicAuthenticator(policy.Username, policy.Password);
            var request = new RestRequest(ApiUrls.Users(policy.Project), Method.GET);
            var response = client.Execute(request);
            string contentString = response.Content;
            return Deserialization.JsonDeserialize<List<User>>(contentString);
        }
    }
}
