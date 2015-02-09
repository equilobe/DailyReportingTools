using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class IndividualReportProcessor : BaseReportProcessor
    {
        JiraPolicy Policy { get; set; }
        JiraOptions Options { get; set; }

        public IndividualReportProcessor(JiraPolicy policy, JiraOptions options)
            : base(policy, options)
        {
            Policy = policy;
            Options = options;
        }

        public void ProcessReport(IndividualReport report)
        {
            SaveReport(Policy, report);
            JiraPolicyService.SetIndividualEmail(report.Author.EmailAdress, Policy);
            SendReport(report);
        }

        private void SaveReport(JiraPolicy policy, IndividualReport individualReport)
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\IndividualReportTemplate.cshtml";
            var reportPath = GetReportPath(individualReport);
            SaveReportToFile(individualReport, reportPath, policy, viewPath);
        }

        private string GetReportPath(IndividualReport report)
        {
            string reportPath = report.Policy.GeneratedProperties.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, report.Author.Name + "_" + report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        private void SendReport(IndividualReport report)
        {
            var emailer = new ReportEmailerJira(report.Policy, Options);
            emailer.Author = report.Author;

            emailer.TrySendEmails();
        }
    }
}
