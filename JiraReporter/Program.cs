using CommandLine;
using JiraReporter.Model;
using RazorEngine;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            SourceControlLogReporter.Options options = GetCommandLineOptions(args);
            SourceControlLogReporter.Model.Policy policy = SourceControlLogReporter.Model.Policy.CreateFromFile(options.PolicyPath);
            LoadReportDates(policy, options);
            policy.SetPermanentTaskLabel();
            policy.SetDraftMode(options);
            if (RunReport(policy, options) == true)
                RunReportTool(args, policy, options);
        }

        private static bool RunReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var today = DateTime.Now.ToOriginalTimeZone().Date;
            if (policy.IsDraft == false && today == policy.LastReportSentDate.Date)
                return false;
            if (policy.IsDraft == true && today == policy.LastReportSentDate.Date)
                return false;
            if (policy.IsWeekendReportActive == true && options.IsWeekend() == true)
                return false;
            if (CheckDayFromOverrides(policy) == true)
                return false;
            return true;
        }

        private static bool CheckDayFromOverrides(SourceControlLogReporter.Model.Policy policy)
        {
            if (policy.CurrentOverride != null && policy.CurrentOverride.NonWorkingDays != null)
                return policy.CurrentOverride.NonWorkingDays.Exists(a => a == DateTime.Now.ToOriginalTimeZone().Day);
            return false;
        }

        private static void RunReportTool(string[] args, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            SetTemplateGlobal();

            var report = ReportGenerator.GenerateReport(policy, options);

            SaveReportToFile(report);

            SendReport(report);
        }

        private static void SetTemplateGlobal()
        {
            Razor.SetTemplateBase(typeof(SourceControlLogReporter.RazorEngine.ExtendedTemplateBase<>));
        }

        private static SourceControlLogReporter.Options GetCommandLineOptions(string[] args)
        {
            SourceControlLogReporter.Options options = new SourceControlLogReporter.Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }

        private static void SaveReportToFile(Report report)
        {
            string reportPath = GetReportPath(report);

            var repCont = ReportProcessor.ProcessReport(report);
            SourceControlLogReporter.Reporter.WriteReport(report.Policy, repCont, reportPath);
        }

        private static void SendReport(Report report)
        {
            var emailer = new SourceControlLogReporter.ReportEmailer(report.Policy, report.Options);
            emailer.TrySendEmails();
        }

        private static string GetReportPath(Report report)
        {
            string reportPath = report.Policy.ReportsPath;
            Directory.CreateDirectory(reportPath);
            reportPath = Path.Combine(reportPath, report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

        private static void LoadReportDates(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetSample = RestApiRequests.GetTimesheet(policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(timesheetSample.StartDate);
            options.LoadDates(policy);
        }

    }
}
