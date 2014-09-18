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
            Options options = GetCommandLineOptions(args);
            Policy policy = Policy.CreateFromFile(options.PolicyPath);
            policy.SetPermanentTaskLabel();

            options.LoadDates();
            
            var timesheet = RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate);
            var timesheetService = new TimesheetService();
            timesheetService.SetTimesheetIssues(timesheet, policy, options);

            var report = GetReport(timesheet,policy,options);
            SaveReportToFile(report);

            SendReport(report, GetReportPath(report));
        }

        private static Options GetCommandLineOptions(string[] args)
        {
            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }   
 
        private static Report GetReport(Timesheet timesheet, Policy policy, Options options)
        {           
            var sprint = GetSprintReport(policy, options, timesheet);
            var authors = AuthorsProcessing.GetAuthors(timesheet, sprint, policy);
            var report = new Report(policy, options) { Authors = authors, Sprint=sprint, Date = options.FromDate, Summary=new Summary(authors,sprint)};
            return report;
        }

        private static SprintStatus GetSprintReport(Policy p, Options options, Timesheet timesheet)
        {
            var report = new SprintStatus();
            report.SetSprintTasks(p, timesheet, options);
            return report;
        }

        private static void SaveReportToFile(Report report)
        {
            string reportPath = GetReportPath(report);

            var repCont = ReportProcessor.ProcessReport(report);

            File.WriteAllText(reportPath, repCont);
        }

        private static void SendReport(Report report, string reportPath)
        {
            var emailer = new ReportEmailer(report.policy, report.options);
            emailer.EmailReport(reportPath);
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
