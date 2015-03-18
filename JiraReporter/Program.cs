using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using RazorEngine;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SourceControlLogReporter;
using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Services;
using JiraReporter.Helpers;
using CommandLine;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            new JiraReportMainFlowProcessor().Execute(args);
        }

    }
}
