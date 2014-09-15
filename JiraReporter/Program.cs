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
            Policy p = Policy.CreateFromFile(options.PolicyPath);
            p.SetPermanentTaskLabel();
            options.LoadDates();

            var timesheetService = new TimesheetService();
            var timesheet = timesheetService.GetTimesheet(p, options.FromDate, options.ToDate);

            timesheetService.SetTimesheetIssues(timesheet, p, options);

            var report = GetReport(timesheet,p,options);
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
 
        private static Report GetReport(Timesheet timesheet, Policy p, Options options)
        {
            var authors = AuthorsProcessing.GetAuthors(timesheet);
            var report = new Report(p, options) { Authors = authors, Summary = authors, SprintReport=GetSprintReport(p, options, timesheet)};
            SetReport(report);
            return report;
        }

        private static SprintStatusReport GetSprintReport(Policy p, Options options, Timesheet timesheet)
        {
            var report = new SprintStatusReport();
            report.SetSprintTasks(p, timesheet, options);
            return report;
        }

        private static void SetReport(Report report)
        {
            ReportProcessor.SetReportTimes(report);
            report.Authors = AuthorsProcessing.OrderAuthorsName(report.Authors);
            report.Summary = AuthorsProcessing.OrderAuthorsTime(report.Summary);
            report.Date = report.options.FromDate;
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
