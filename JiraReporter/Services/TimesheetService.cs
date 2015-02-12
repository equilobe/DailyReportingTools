using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.JiraOriginals;
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

namespace JiraReporter.Services
{
    public class TimesheetService
    {
        public Timesheet LoadTimeSheet(string xmlString)
        {
            var reader = new StringReader(xmlString);
            var serializer = new XmlSerializer(typeof(Timesheet));
            return (Timesheet)serializer.Deserialize(reader);
        }

        //public void SetTimesheetIssues(Timesheet timesheet, JiraReport context)
        //{
        //    if (timesheet == null)
        //        return;

        //    var issues = new List<CompleteIssue>(timesheet.Worklog.Issues);
        //    var issueProcessor = new IssueProcessor(context);
        //    issueProcessor.SetIssues(issues);
        //}

        public static int GetTotalOriginalEstimate(List<CompleteIssue> issues)
        {
            int sum = 0;
            if (issues == null)
                return 0;

            return sum = issues.Sum(i => i.OriginalEstimateSeconds);
        }

        public static List<CompleteIssue> GetIssuesFromJiraTimesheet(List<JiraIssueSmall> jiraIssues)
        {
            var issues = new List<CompleteIssue>();
            foreach (var issue in jiraIssues)
                issues.Add(new CompleteIssue
                {
                    Key = issue.Key,
                    Entries = issue.Entries,
                    Summary = issue.Summary
                });
            return issues;
        }
    }
}
