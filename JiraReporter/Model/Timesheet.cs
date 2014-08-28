using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter.Model
{
    [XmlRoot("rawTimeSheetRepresentation")]
    public class Timesheet
    {
        [XmlElement("worklog", Type = typeof(WorkLog))]
        public WorkLog Worklog;

        [XmlElement("startDate")]
        public DateTime StartDate;

        [XmlElement("endDate")]
        public DateTime EndDate;

        public static Timesheet LoadTimeSheet(string xmlString)
        {
            var reader = new StringReader(xmlString);
            var serializer = new XmlSerializer(typeof(Timesheet));
            return (Timesheet)serializer.Deserialize(reader);
        }

        private static void Authenticate(RestClient client, string username, string password)
        {
            client.Authenticator = new HttpBasicAuthenticator(username, password);
        }

        public Timesheet GetTimesheet(string targetGroup, DateTime startDate, DateTime endDate)
        {
            string fromDate = Options.DateToString(startDate);
            string toDate = Options.DateToString(endDate);
            var client = new RestClient("https://equilobe.atlassian.net");
            Authenticate(client, LoginUtils.Username, LoginUtils.Password);
            var request = new RestRequest("/rest/timesheet-gadget/1.0/raw-timesheet.xml?targetGroup={0}&startDate={1}&endDate={2}", Method.GET);
            request.AddUrlSegment("0", targetGroup);
            request.AddUrlSegment("1", fromDate);
            request.AddUrlSegment("2", toDate);
            var response = client.Execute(request);
            string xmlString = response.Content;
            return Timesheet.LoadTimeSheet(xmlString);
        }

        public static string SetTimeFormat(int time)
        {
            string timeFormat="";
          
            TimeSpan t = TimeSpan.FromSeconds(time);

            if(t.Hours>0)
                 timeFormat += " " + string.Format("{0}h", t.Hours + t.Days*24);
            if (t.Minutes > 0)
                 timeFormat += " " + string.Format("{0}m", t.Minutes);
            return timeFormat;
        }
    }
}
