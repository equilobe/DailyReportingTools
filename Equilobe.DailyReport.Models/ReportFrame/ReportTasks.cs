﻿using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class ReportTasks
    {
        public List<IssueDetailed> CompletedTasksAll { get; set; }
        public List<IssueDetailed> CompletedTasksVisible { get; set; }
        public int AdditionalCompletedTasks { get; set; }
        public int AdditionalUnassignedTasks { get; set; }
        public Uri CompletedTasksSearchUrl { get; set; }
        public Uri UnassignedTasksSearchUrl { get; set; }

        public List<IssueDetailed> InProgressTasks { get; set; }
        public List<IssueDetailed> OpenTasks { get; set; }
        public List<IssueDetailed> UnassignedTasksAll { get; set; }
        public List<IssueDetailed> UnassignedTasksVisible { get; set; }
        public List<IssueDetailed> UncompletedTasks { get; set; }
        public List<IssueDetailed> SprintTasksAll { get; set; }
        public List<JiraIssue> FutureSprintTasks { get; set; }
        public List<JiraIssue> PastSprintTasks { get; set; }
        public int UnassignedTasksErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }  
    }
}
