using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utilsr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Helpers
{
    public static class JiraReportHelpers
    {
        public static string GetReportTitle(JiraReport report)
        {
            var title = string.Empty;
            
            if(report.IsIndividualDraft)
            {
                title += report.Author.Name + " - ";
            }

            title += report.ProjectName;

            return title;
        }

        public static string GetReportSubject(JiraReport report)
        {
            var subject = "DailyReport | ";
            if(report.IsIndividualDraft || report.IsFinalDraft)
            {
                subject += "DRAFT | ";
            }
            if(report.IsIndividualDraft)
            {
                subject += report.Author.Name + " - ";
            }
            subject += " " + report.ProjectName + " | " + report.ToDate.ToString("ddd, dd MMM yyyy");

            return subject;
        }

        public static JiraRequestContext GetJiraRequestContext(JiraReport report)
        {
            report.Policy.Username = report.Settings.InstalledInstance.JiraUsername;
            report.Policy.Password = report.Settings.InstalledInstance.JiraPassword;

            return new JiraRequestContext(report.Settings.InstalledInstance);
        }
    }
}
