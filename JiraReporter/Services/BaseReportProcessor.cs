using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Utils;
using JiraReporter.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JiraReporter
{
    public class BaseReportProcessor
    {
        protected JiraPolicy Policy { get { return Report.Policy; } }
        protected JiraOptions Options { get { return Report.Options; } }
        protected JiraReport Report { get; set; }


        public BaseReportProcessor(JiraReport report)
        {
            Report = report;
        }

        public virtual void ProcessReport()
        {
            SaveReport();
            SetEmailCollection(Report.Authors);

            SendReport();
        }

        protected virtual void SaveReport()
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\TimesheetReportTemplate.cshtml";
            var reportPath = GetReportPath();
            SaveReportToFile(reportPath, viewPath);
        }

        protected virtual string GetReportPath()
        {
            string reportPath = Report.ReportsPath;
            Validations.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, Report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        protected void SaveReportToFile(string reportPath, string viewPath)
        {
            var repCont = SourceControlLogReporter.ReportBase.ProcessReport(Report, viewPath);
            WriteReport(Report, repCont, reportPath);
        }

        protected virtual void SendReport()
        {
            var emailer = new ReportEmailerJira(Report);

            emailer.TrySendEmails();
        }

        protected void SetEmailCollection(List<JiraAuthor> authors)
        {
            Policy.EmailCollection = new List<string>();

            if (Report.IsFinalDraft)
                SetDraftEmailCollection(authors);
            else
                SetFinalReportEmailCollection(authors);

            Policy.EmailCollection = Policy.EmailCollection.Distinct().ToList();
        }

        private void SetFinalReportEmailCollection(List<JiraAuthor> authors)
        {
            if (Policy.AdvancedOptions.SendFinalToOthers)
                Policy.EmailCollection = JiraContextService.GetFinalAddedEmails(Policy);
            if (Policy.AdvancedOptions.SendFinalToAllUsers)
                AddUsersEmailAdresses(authors);
        }

        private void SetDraftEmailCollection(List<JiraAuthor> authors)
        {
            if (Policy.AdvancedOptions.SendDraftToOthers)
                Policy.EmailCollection = JiraContextService.GetDraftAddedEmails(Policy);
            if (!Policy.AdvancedOptions.SendDraftToAllUsers && Policy.AdvancedOptions.SendDraftToProjectManager)
                Policy.EmailCollection.Add(authors.Find(a => a.IsProjectLead).EmailAdress);
            if (Policy.AdvancedOptions.SendDraftToAllUsers)
                AddUsersEmailAdresses(authors);
        }

        private void AddUsersEmailAdresses(List<JiraAuthor> authors)
        {
            foreach (var author in authors)
                if (author.EmailAdress != null)
                    Policy.EmailCollection.Add(author.EmailAdress);
        }

        private static void WriteReport(JiraReport context, string report, string path)
        {
            Validations.EnsureDirectoryExists(context.LogArchivePath);

            var archivedFilePath = Path.Combine(context.LogArchivePath, Path.GetFileName(path));

            if (File.Exists(path))
            {
                File.Copy(path, archivedFilePath, true);
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(archivedFilePath, report);
            }

            Validations.EnsureDirectoryExists(context.UnsentReportsPath);

            var reportPath = Path.Combine(context.UnsentReportsPath, Path.GetFileNameWithoutExtension(path) + ".html");

            File.WriteAllText(reportPath, report);
        }
    }
}
