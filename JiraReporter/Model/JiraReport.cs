using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class JiraReport
    {
        public JiraPolicy Policy;
        public JiraOptions Options;

        public JiraReport(JiraPolicy p, JiraOptions o)
        {
            Policy = p;
            Options = o;
        }
        public string Title { get; set; }
        public List<JiraAuthor> Authors { get; set; }
        public DateTime Date { get; set; }
        public Summary Summary { get; set; }
        public SprintTasks SprintTasks { get; set; }
        public List<JiraPullRequest> PullRequests { get; set; }
        public List<JiraPullRequest> UnrelatedPullRequests
        {
            get
            {
                return PullRequests.FindAll(p => p.TaskSynced == false);
            }
        }

        public virtual string GetReportTitle()
        {
            var title = string.Empty;
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(Options.FromDate, Options.ToDate);
            if (Policy.GeneratedProperties.IsFinalDraft)
                title += "DRAFT | ";
            title += Policy.GeneratedProperties.ProjectName + " Daily Report | " + reportDate;

            return title;
        }
    }
}
