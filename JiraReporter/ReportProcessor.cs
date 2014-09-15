using JiraReporter.Model;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ReportProcessor
    {
        public static string ProcessReport(Report report)
        {
            string template = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\Views\TimesheetReportTemplate.cshtml");
            report.Title = report.policy.ReportTitle;
            return Razor.Parse(template, report);
        }

        public static void SetReportTimes(Report report)
        {
            AuthorsProcessing.SetAuthorsTimeSpent(report.Authors);
            report.TotalTime = TimeFormatting.SetReportTotalTime(report.Authors);
            AuthorsProcessing.SetAuthorsTimeFormat(report.Authors);
        }
    }
}
