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
            //string baseUrl = "/rest/timesheet-gadget/1.0/raw-timesheet.xml";
            Options options = GetCommandLineOptions(args);
            Policy p = Policy.CreateFromFile(options.PolicyPath);
            options.LoadDates();
            var timesheet = new Timesheet();
            timesheet = timesheet.GetTimesheet(p.TargetGroup, options.FromDate, options.ToDate);
            var issues = new List<Issue>(timesheet.Worklog.Issues);

            foreach (var issue in issues)
                Issue.SetEntries(issue.Entries, issue, timesheet.Worklog.Issues);
            Issue.RemoveEntries(timesheet.Worklog.Issues);
            Issue.SetIssues(timesheet);

            var authors = Author.GetAuthors(timesheet);
            var report = new Report(p, options) { Authors = authors, Summary=authors };

            report.SetReportTimes();

            report.Authors = Author.OrderAuthorsName(report.Authors);
            report.Summary = Author.OrderAuthorsTime(report.Summary);

            report.Date = options.FromDate;

            string reportPath = p.ReportsPath;
            Directory.CreateDirectory(reportPath);

            reportPath = Path.Combine(reportPath, report.Date.ToString("yyyy-MM-dd") + ".html");

            var repCont = ReportProcessor.ProcessReport(report);

            File.WriteAllText(reportPath, repCont);

            var emailer = new ReportEmailer(p, options);
            emailer.EmailReport(reportPath);
        }
          
        private static Options GetCommandLineOptions(string[] args)
        {
            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }
    }
}
