using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Services;
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
        public IndividualReportProcessor(JiraReport report)
            : base(report)
        {
        }

        public override void ProcessReport()
        {
            SaveReport();
            JiraPolicyService.SetIndividualEmail(Report.Author.EmailAdress, Policy);
            SendReport();
        }

        protected override void SaveReport()
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\IndividualReportTemplate.cshtml";
            var reportPath = GetReportPath();
            SaveReportToFile(reportPath, viewPath);
        }

        protected override string GetReportPath()
        {
            string reportPath = Policy.GeneratedProperties.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, Report.Author.Name + "_" + Report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }
    }
}
