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
            SvnLogReporter.Options options = GetCommandLineOptions(args);
            SvnLogReporter.Model.Policy policy = SvnLogReporter.Model.Policy.CreateFromFile(options.PolicyPath);
            policy.SetPermanentTaskLabel();
            SetTemplateGlobal();

            options.LoadDates(policy);

            var report = ReportGenerator.GenerateReport(policy, options);
            
            SaveReportToFile(report);

            SendReport(report);
        }

        private static void SetTemplateGlobal()
        {
            Razor.SetTemplateBase(typeof(SvnLogReporter.RazorEngine.ExtendedTemplateBase<>));
        }

        private static SvnLogReporter.Options GetCommandLineOptions(string[] args)
        {
            SvnLogReporter.Options options = new SvnLogReporter.Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }        

        private static void SaveReportToFile(Report report)
        {
            string reportPath = GetReportPath(report);

            var repCont = ReportProcessor.ProcessReport(report);
            SvnLogReporter.Reporter.WriteReport(report.policy, repCont, reportPath);
        }

        private static void SendReport(Report report)
        {
            var emailer = new SvnLogReporter.ReportEmailer(report.policy, report.options);
            emailer.TrySendEmails();
        }

        private static string GetReportPath(Report report)
        {
            string reportPath = report.policy.ReportsPath;
            Directory.CreateDirectory(reportPath);
            reportPath = Path.Combine(reportPath, report.Date.ToString("yyyy-MM-dd") + ".html");
            return reportPath;
        }

    }
}
