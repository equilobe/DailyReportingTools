using JiraReporter.Model;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
namespace JiraReporter
{
    class ReportProcessor
    {
        public static string ProcessReport(Report report)
        {
            string viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\TimesheetReportTemplate.cshtml";
            return SourceControlLogReporter.ReportBase.ProcessReport(report, viewPath);
        }      
    }
}
