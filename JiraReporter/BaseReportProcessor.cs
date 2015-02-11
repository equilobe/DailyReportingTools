using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using JiraReporter.Services;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //    SetEmailCollection(report.Authors);
            Policy.EmailCollection = new List<string>();
            Policy.EmailCollection.Add("sebastian.dumitrascu@equilobe.com");
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
            string reportPath = Policy.GeneratedProperties.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, Report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        protected void SaveReportToFile(string reportPath, string viewPath)
        {
            var repCont = SourceControlLogReporter.ReportBase.ProcessReport(Report, viewPath);
            WriteReport(Policy, repCont, reportPath);
        }

        protected virtual void SendReport()
        {
            var emailer = new ReportEmailerJira(Report);

            emailer.TrySendEmails();
        }

        protected void SetEmailCollection(List<JiraAuthor> authors)
        {
            Policy.EmailCollection = new List<string>();

            if (Policy.GeneratedProperties.IsFinalDraft)
                SetDraftEmailCollection(authors);
            else
                SetFinalReportEmailCollection(authors);

            Policy.EmailCollection = Policy.EmailCollection.Distinct().ToList();
        }

        private void SetFinalReportEmailCollection(List<JiraAuthor> authors)
        {
            if (Policy.AdvancedOptions.SendFinalToOthers)
                Policy.EmailCollection = JiraPolicyService.GetFinalAddedEmails(Policy);
            if (Policy.AdvancedOptions.SendFinalToAllUsers)
                AddUsersEmailAdresses(authors);
        }

        private void SetDraftEmailCollection(List<JiraAuthor> authors)
        {
            if (Policy.AdvancedOptions.SendDraftToOthers)
                Policy.EmailCollection = JiraPolicyService.GetDraftAddedEmails(Policy);
            if (!Policy.AdvancedOptions.SendDraftToAllUsers && Policy.AdvancedOptions.SendDraftToProjectManager)
                Policy.EmailCollection.Add(authors.Find(a => a.IsProjectLead).EmailAdress);
            else
                AddUsersEmailAdresses(authors);
        }

        private void AddUsersEmailAdresses(List<JiraAuthor> authors)
        {
            foreach (var author in authors)
                if (author.EmailAdress != null)
                    Policy.EmailCollection.Add(author.EmailAdress);
        }

        private static void WriteReport(JiraPolicy policy, string report, string path)
        {
            Validation.EnsureDirectoryExists(policy.GeneratedProperties.LogArchivePath);

            var archivedFilePath = Path.Combine(policy.GeneratedProperties.LogArchivePath, Path.GetFileName(path));

            if (File.Exists(path))
            {
                File.Copy(path, archivedFilePath, true);
                File.Delete(path);
            }
            else
            {
                File.WriteAllText(archivedFilePath, report);
            }

            Validation.EnsureDirectoryExists(policy.GeneratedProperties.UnsentReportsPath);

            var reportPath = Path.Combine(policy.GeneratedProperties.UnsentReportsPath, Path.GetFileNameWithoutExtension(path) + ".html");

            File.WriteAllText(reportPath, report);
        }
    }
}
