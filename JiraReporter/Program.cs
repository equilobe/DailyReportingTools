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
            options.LoadDates();

            var timesheetService = new TimesheetService();
            var timesheet = timesheetService.GetTimesheet(p, options.FromDate, options.ToDate);

            SetTimesheetIssues(timesheet, p, options);

            var report = GetReport(timesheet,p,options);

            SendReport(report);
        }

        private static Options GetCommandLineOptions(string[] args)
        {
            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }

        private static void SetTimesheetIssues(Timesheet timesheet, Policy policy, Options options)
        {
            var issues = new List<Issue>(timesheet.Worklog.Issues);
            foreach (var issue in issues)
                Issue.SetEntries(issue.Entries, issue, timesheet.Worklog.Issues);
            Issue.RemoveEntries(timesheet.Worklog.Issues);
            Issue.SetIssues(timesheet, policy, options);
        }     
 
        private static Report GetReport(Timesheet timesheet, Policy p, Options options)
        {
            var authors = Author.GetAuthors(timesheet);
            var report = new Report(p, options) { Authors = authors, Summary = authors, SprintReport=GetSprintReport(p, options, timesheet)};
            SetReport(report);
            return report;
        }

        private static SprintReport GetSprintReport(Policy p, Options options, Timesheet timesheet)
        {
            var report = new SprintReport();
            report.GetOldCompletedTasks(p, options);
            report.GetRecentlyCompletedTasks(p, options, timesheet);
            report.SetSprintTasks(p);
            return report;
        }

        private static void SetReport(Report report)
        {
            report.SetReportTimes();
            report.Authors = Author.OrderAuthorsName(report.Authors);
            report.Summary = Author.OrderAuthorsTime(report.Summary);
            report.Date = report.options.FromDate;
            report.SprintReport.SortTasks();
        }

        private static void SendReport(Report report)
        {
            string reportPath = GetReportPath(report);

            var repCont = ReportProcessor.ProcessReport(report);

            File.WriteAllText(reportPath, repCont);

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
