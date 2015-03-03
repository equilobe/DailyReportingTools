using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
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

        public static JiraRequestContext GetJiraRequestContext(JiraReport report)
        {
        //    policy.SharedSecret = DbService.GetSharedSecret(policy.BaseUrl);
            //set username and password

            if (!string.IsNullOrEmpty(report.Policy.SharedSecret))
                return new JiraRequestContext(report.Policy.BaseUrl, report.Policy.SharedSecret);

            report.Policy.Username = report.Settings.Username;
            report.Policy.Password = report.Settings.Password;

            return new JiraRequestContext(report.Policy.BaseUrl, report.Settings.Username, report.Settings.Password);
        }
    }
}
