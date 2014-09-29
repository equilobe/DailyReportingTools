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

            options.LoadDates();           

            var report = ReportGenerator.GenerateReport(policy, options);
            
            SaveReportToFile(report);

            SendReport(report, GetReportPath(report));
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

            File.WriteAllText(reportPath, repCont);
        }

        private static void SendReport(Report report, string reportPath)
        {
            var emailer = new SvnLogReporter.ReportEmailer(report.policy, report.options);
            emailer.TryEmailReport(reportPath);
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
