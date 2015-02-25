using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Helpers
{
    public static class JiraReportHelpers
    {
        public static string GetReportTitle(JiraReport report, bool individualReport = false)
        {
            var title = string.Empty;
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(report.Options.FromDate, report.Options.ToDate);
            if (report.IsFinalDraft || report.IsIndividualDraft)
                title += "DRAFT | ";
            if (individualReport)
                title += report.Author.Name + " | ";
            title += report.ProjectName + " Daily Report | " + reportDate;

            return title;
        }

        public static JiraRequestContext GetJiraRequestContext(JiraPolicy policy)
        {
            if (policy.SharedSecret != null)
                return new JiraRequestContext(policy.BaseUrl, policy.SharedSecret);

            return new JiraRequestContext(policy.BaseUrl, policy.Username, policy.Password);
        }
    }
}
