using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using RestSharp;
using SourceControlLogReporter;
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

        public void SetTimesheetIssues(Timesheet timesheet, JiraPolicy policy, List<JiraPullRequest> pullRequests)
        {
            if (timesheet == null)
                return;

            var issues = new List<Issue>(timesheet.Worklog.Issues);
            var issueProcessor = new IssueProcessor(policy, pullRequests);
            issueProcessor.SetIssues(issues);
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
