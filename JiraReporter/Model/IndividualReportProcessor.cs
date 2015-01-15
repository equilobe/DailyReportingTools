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
        SourceControlLogReporter.Model.Policy Policy { get; set; }
        SourceControlLogReporter.Options Options { get; set; }

        public IndividualReportProcessor(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options) : base(policy, options)
        {
            Policy = policy;
            Options = options;
        }

        public void ProcessReport(IndividualReport report)
        {
            SaveReport(Policy, report);
            Policy.SetIndividualEmail("sebastian.dumitrascu@equilobe.com");
            SendReport(report);
        }

        private void SaveReport(SourceControlLogReporter.Model.Policy policy, IndividualReport individualReport)
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\IndividualReportTemplate.cshtml";
            var reportPath = GetReportPath(individualReport);
            SaveReportToFile(individualReport, reportPath, policy, viewPath);
        }

        private string GetReportPath(IndividualReport report)
        {
            string reportPath = report.Policy.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, report.Author.Name + "_" + report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        private void SendReport(IndividualReport report)
        {
            var emailer = new ReportEmailJira(report.Policy, report.Options);
            emailer.Author = report.Author;

            emailer.TrySendEmails();
        }
    }
}
