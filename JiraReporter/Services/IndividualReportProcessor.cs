using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
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
            JiraContextService.SetIndividualEmail(Report.Author.EmailAdress, Policy);
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
            string reportPath = Report.ReportsPath;
            Validations.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, Report.Author.Name + "_" + Report.ReportDate.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }
    }
}
