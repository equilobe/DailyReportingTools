using Equilobe.DailyReport.DAL;
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
            //  policy.SharedSecret = SecretKeyProviderFactory.GetSecretKeyProvider().GetSecretKey(policy.BaseUrl);

            //if (policy.SharedSecret != null)
            //    return new JiraRequestContext(policy.BaseUrl, policy.SharedSecret);

            //return new JiraRequestContext(policy.BaseUrl, policy.Username, policy.Password);
            policy.Username = "sebastian.dumitrascu";
            policy.Password = "clatite";
            return new JiraRequestContext(policy.BaseUrl, policy.Username, policy.Password);
        }

        //TODO: move this method to db services
        public static void SetReportFromDb(JiraReport report)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == report.UniqueProjectKey);
                report.Settings = reportSettings;
                if (reportSettings.ReportExecutionSummary != null)
                {
                    if (reportSettings.ReportExecutionSummary.LastDraftSentDate != null)
                        report.LastDraftSentDate = reportSettings.ReportExecutionSummary.LastDraftSentDate.Value;
                    if (reportSettings.ReportExecutionSummary.LastFinalReportSentDate != null)
                        report.LastReportSentDate = reportSettings.ReportExecutionSummary.LastFinalReportSentDate.Value;
                }

                if (reportSettings.FinalDraftConfirmation != null)
                    if (reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate != null)
                        report.LastFinalDraftConfirmationDate = reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate.Value;
            }
        }
    }
}
