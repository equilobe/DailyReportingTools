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

        public void SetTimesheetIssues(Timesheet timesheet, SvnLogReporter.Model.Policy policy, Options options)
        {
            var issues = new List<Issue>(timesheet.Worklog.Issues);
            foreach (var issue in issues)
                Issue.SetEntries(issue.Entries, issue, timesheet.Worklog.Issues);
            Issue.RemoveEntries(timesheet.Worklog.Issues);
            Issue.SetIssues(timesheet, policy, options);
        }  
    }
}
