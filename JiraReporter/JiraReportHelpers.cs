using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class JiraReportHelpers
    {
        public static string GetReportTitle(DateTime fromDate, DateTime toDate, JiraPolicy policy, string author = "")
        {
            var title = string.Empty;
            var reportDate = SourceControlLogReporter.ReportDateFormatter.GetReportDate(fromDate, toDate);
            if (policy.GeneratedProperties.IsFinalDraft || policy.GeneratedProperties.IsIndividualDraft)
                title += "DRAFT | ";
            if (!string.IsNullOrEmpty(author))
                title += author + " | ";
            title += policy.GeneratedProperties.ProjectName + " Daily Report | " + reportDate;

            return title;
        }
    }
}
