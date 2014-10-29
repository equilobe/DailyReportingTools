using JiraReporter.Model;
using RestSharp;
using SvnLogReporter;
using SvnLogReporter.Model;
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

        public void SetTimesheetIssues(Timesheet timesheet, Policy policy, Options options, List<PullRequest> pullRequests)
        {
            var issues = new List<Issue>(timesheet.Worklog.Issues);
            foreach (var issue in issues)
                IssueAdapter.SetIssueEntries(issue.Entries, issue, timesheet.Worklog.Issues);
            IssueAdapter.RemoveWrongEntries(timesheet.Worklog.Issues);
            IssueAdapter.SetIssues(timesheet, policy, options, pullRequests);
        }
  
        public Timesheet GenerateMonthTimesheet(Options options, Policy policy)
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month,1);
            return RestApiRequests.GetTimesheet(policy, startOfMonth, DateTime.Now);
        }

        public Timesheet GenerateTimehseet(Options options, Policy policy)
        {
            return RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1));
        }
    }
}
