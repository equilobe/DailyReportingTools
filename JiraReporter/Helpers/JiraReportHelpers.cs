using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Helpers
{
    public static class JiraReportHelpers
    {
        public static string GetReportTitle(JiraReport report, bool isIndividualReportTitle = false)
        {
            var title = string.Empty;

            if (isIndividualReportTitle)
            {
                title += report.Author.Name + " - ";
            }

            title += report.Policy.AdvancedOptions.ReportTitle;

            return title;
        }

        public static string GetReportSubject(JiraReport report)
        {
            var count = GetDraftCount(report);
            var subject = "DailyReport | ";
            if (report.IsIndividualDraft || report.IsFinalDraft)
            {
                subject += "DRAFT ";
                if (count > 1)
                    subject += count + " ";
                subject += " | ";
            }
            else
            {
                subject += "FINAL |";
            }
            if (report.IsIndividualDraft)
            {
                subject += report.Author.Name + " - ";
            }
            subject += " " + report.Policy.AdvancedOptions.ReportTitle + " | " + report.Date;

            return subject;
        }

        public static JiraRequestContext GetJiraRequestContext(JiraReport report)
        {
            report.Policy.Username = report.Settings.InstalledInstance.JiraUsername;
            report.Policy.Password = report.Settings.InstalledInstance.JiraPassword;

            return new JiraRequestContext(report.Settings.InstalledInstance);
        }

        public static int GetDraftCount(JiraReport report)
        {
            int count = 0;
            if (report.IsIndividualDraft)
                count = report.Settings.ReportExecutionInstances.Count(i => CountIndividualDraft(i, report));
            else if (report.IsFinalDraft)
                count = report.Settings.ReportExecutionInstances.Count(i => CountFinalDraft(i, report));

            return count + 1;
        }

        static bool CountIndividualDraft(ReportExecutionInstance instance, JiraReport report)
        {
            //var individualDraft = report.Settings.IndividualDraftConfirmations.Single(dr => dr.UniqueUserKey == instance.UniqueUserKey);

            return instance.BasicSettingsId == report.Settings.Id && instance.DateExecuted != null && instance.DateExecuted.Value.Date == DateTime.Today && instance.Scope == SendScope.SendIndividualDraft && instance.Status == "Succes" && report.Author.IndividualDraftInfo.UniqueUserKey == instance.UniqueUserKey;
        }

        static bool CountFinalDraft(ReportExecutionInstance instance, JiraReport report)
        {
            return instance.BasicSettingsId == report.Settings.Id && instance.DateExecuted != null && instance.DateExecuted.Value.Date == DateTime.Today && instance.Scope == SendScope.SendFinalDraft && instance.Status == "Succes";
        }
    }
}
