﻿using Equilobe.DailyReport.Models.Jira;
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

        public void ProcessReport(JiraReport report)
        {
            SaveReport(Policy, report);
            JiraPolicyService.SetIndividualEmail(report.Author.EmailAdress, Policy);
            SendReport(report);
        }

        private void SaveReport(JiraReport individualReport)
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\IndividualReportTemplate.cshtml";
            var reportPath = GetReportPath(individualReport);
            SaveReportToFile(individualReport, reportPath, viewPath);
        }

        private string GetReportPath(JiraReport report)
        {
            string reportPath = report.Policy.GeneratedProperties.ReportsPath;
            SourceControlLogReporter.Validation.EnsureDirectoryExists(reportPath);
            reportPath = Path.Combine(reportPath, report.Author.Name + "_" + report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        private void SendReport(JiraReport report)
        {
            var emailer = new ReportEmailerJira(report.Policy, Options);
            emailer.Author = report.Author;

            emailer.TrySendEmails();
        }
    }
}