﻿using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraReport : IContext
    {
        List<JiraAuthor> Authors { get; set; }
        List<JiraCommit> Commits { get; set; }
        string Date { get; set; }
        DateTime ReportDate { get; }
        DateTime FromDate { get; }
        List<JiraPullRequest> PullRequests { get; set; }
        Sprint Sprint { get; set; }
        ReportTasks ReportTasks { get; set; }
        Summary Summary { get; set; }
        string Title { get; set; }
        DateTime ToDate { get; }
        List<JiraPullRequest> UnrelatedPullRequests { get; }
    }
}
