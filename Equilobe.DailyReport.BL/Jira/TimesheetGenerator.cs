﻿using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL.Jira
{
    public class TimesheetGenerator
    {
        JiraClient Client { get; set; }

        public TimesheetGenerator(JiraClient client)
        {
            Client = client;
        }

        public List<JiraIssue> GetTimesheetIssuesForAuthor(string projectKey, string author, DateTime fromDate, DateTime toDate)
        {
            var updatedIssues = Client.GetWorklogs(projectKey, author, fromDate.ToString("yyyy/MM/dd"), toDate.ToString("yyyy/MM/dd"));
            var timesheetIssues = updatedIssues
                .Select(issue => Client.GetIssue(issue.key))
                .ToList();
            timesheetIssues.ForEach(issue => issue.fields.worklog = Client.GetIssueWorklogs(issue.key));

            return timesheetIssues;
        }
    }
}
