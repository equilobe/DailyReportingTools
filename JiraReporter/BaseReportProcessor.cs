using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
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
        JiraPolicy Policy { get; set; }
        JiraOptions Options { get; set; }

        public BaseReportProcessor(JiraPolicy policy, JiraOptions options)
        {
            Policy = policy;
            Options = options;
        }

        public virtual void ProcessReport(JiraReport report)
        {
            SaveReport(Policy, report);
            //      Policy.SetEmailCollection();
            SetEmailCollection(report.Authors);
            SendReport(report);
        }

        protected virtual void SaveReport(JiraPolicy policy, JiraReport report)
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\TimesheetReportTemplate.cshtml";
            var reportPath = GetReportPath(report);
            SaveReportToFile(report, reportPath, policy, viewPath);
        }

        protected virtual string GetReportPath(JiraReport report)
        {
            string reportPath = report.Policy.GeneratedProperties.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        protected void SaveReportToFile<T>(T report, string reportPath, JiraPolicy policy, string viewPath)
        {
            var repCont = SourceControlLogReporter.ReportBase.ProcessReport(report, viewPath);
            WriteReport(policy, repCont, reportPath);
        }

        protected virtual void SendReport(JiraReport report)
        {
            var emailer = new ReportEmailerJira(report.Policy, report.Options);
            emailer.Authors = report.Authors;

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
