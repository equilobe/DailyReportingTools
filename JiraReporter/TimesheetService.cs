﻿using JiraReporter.Model;
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
            IssueAdapter.SetIssues(issues, policy, options, pullRequests);
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
