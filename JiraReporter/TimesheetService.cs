using JiraReporter.Model;
using RestSharp;
using SourceControlLogReporter;
using SourceControlLogReporter.Model;
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

        public void SetTimesheetCollection(Dictionary<TimesheetType, Timesheet> timesheetCollection, Policy policy, Options options, List<PullRequest> pullRequests)
        {
            if (timesheetCollection != null)
                foreach (var timesheet in timesheetCollection)
                    SetTimesheetIssues(timesheet.Value, policy, options, pullRequests);
        }

        public int GetTotalOriginalEstimate(Timesheet timesheet)
        {
            int sum = 0;
            if (timesheet != null && timesheet.Worklog != null && timesheet.Worklog.Issues != null)
            {
                var issues = timesheet.Worklog.Issues;
                sum = issues.Sum(i => i.OriginalEstimateSeconds);
            }
            return sum;
        }
    }
}
