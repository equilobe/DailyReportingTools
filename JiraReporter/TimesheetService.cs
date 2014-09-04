using JiraReporter.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter
{
    public class TimesheetService
    {
        public Timesheet LoadTimeSheet(string xmlString)
        {
            var reader = new StringReader(xmlString);
            var serializer = new XmlSerializer(typeof(Timesheet));
            return (Timesheet)serializer.Deserialize(reader);
        }

        private static void Authenticate(RestClient client, string username, string password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);
        }

        public Timesheet GetTimesheet(Policy policy, DateTime startDate, DateTime endDate)
        {
            string fromDate = Options.DateToString(startDate);
            string toDate = Options.DateToString(endDate);
            var client = new RestClient(policy.BaseUrl);
            Authenticate(client, policy.Username, policy.Password);
            var request = new RestRequest("/rest/timesheet-gadget/1.0/raw-timesheet.xml?targetGroup={0}&startDate={1}&endDate={2}", Method.GET);
            request.AddUrlSegment("0", policy.TargetGroup);
            request.AddUrlSegment("1", fromDate);
            request.AddUrlSegment("2", toDate);
            var response = client.Execute(request);
            string xmlString = response.Content;
            return LoadTimeSheet(xmlString);
        }

        public static string SetTimeFormat(int time)
        {
            string timeFormat = "";

            TimeSpan t = TimeSpan.FromSeconds(time);

            if (t.Hours > 0 || t.Days > 0)
                timeFormat += string.Format("{0}h", t.Hours + t.Days * 24) + " ";
            if (t.Minutes > 0)
                timeFormat += string.Format("{0}m", t.Minutes);
            if (t.Days == 0 && t.Hours == 0 && t.Minutes == 0)
                timeFormat = string.Format("{0}m", t.Minutes);
            return timeFormat;
        }
    }
}
