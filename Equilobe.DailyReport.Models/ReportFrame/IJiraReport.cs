using System;
using System.Collections.Generic;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Jira;
namespace Equilobe.DailyReport.Models.ReportFrame
{
    public interface IJiraReport : IContext
    {
        List<JiraAuthor> Authors { get; set; }
        List<JiraCommit> Commits { get; set; }
        DateTime Date { get; }
        DateTime FromDate { get; }
        List<JiraPullRequest> PullRequests { get; set; }
        Sprint Sprint { get; set; }
        SprintTasks SprintTasks { get; set; }
        Summary Summary { get; set; }
        string Title { get; set; }
        DateTime ToDate { get; }
        List<JiraPullRequest> UnrelatedPullRequests { get; }
    }
}
