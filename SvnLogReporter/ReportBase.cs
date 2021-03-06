﻿using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Storage;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Utils;
using Equilobe.DailyReport.Models.Policy;
using Autofac;

namespace SourceControlLogReporter
{
    public abstract class ReportBase
    {
        protected Policy Policy { get; private set; }
        protected Options Options { get; private set; }

        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Policy.LogPath, Options.ReportDate);
            }
        }

        public string GetLogFilePath(string logPath, DateTime reportDate)
        {
            return Path.Combine(logPath, reportDate.ToString("yyyy-MM-dd") + ".xml");
        }

        protected ReportBase(Policy p, Options o)
        {
            Policy = p;
            Options = o;
        }

        public ReportBase()
        {

        }

        public static T Create<T>(Policy p, Options o) where T : ReportBase
        {
            var policyParam = new TypedParameter(typeof(Policy), p);
            var optionsParam = new TypedParameter(typeof(Options), o);
            return DependencyInjection.Container.Resolve<T>(policyParam, optionsParam);
        }

        public virtual string TryGenerateReport()
        {
            try
            {
                return GenerateReport();
            }
            catch (SvnNotAvailableException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GenerateReport()
        {
            var log = CreateLog();
            return GetReportContent(log);
        }

        protected string GetReportContent(Log log)
        {
            var reportContent = "";
            var reports = GetReports(log);
            AddPullRequests(reports.Last(), log);
            var viewPath = AppDomain.CurrentDomain.BaseDirectory + @"\Views\ReportTemplate.cshtml";
            foreach (var report in reports)
            {
                report.Title = Policy.ReportTitle;
                reportContent += ProcessReport(report, viewPath);
            }
            return reportContent;
        }

        public static string ProcessReport<T>(T report, string viewPath)
        {
            string template = File.ReadAllText(viewPath);
            return Razor.Parse(template, report);
        }

        protected List<Report> GetReports(Log log)
        {
            var reports = new List<Report>();
            var dayLogs = GetDayLogs(log, Options.ReportDates);
            var report = new Report();
            foreach (var dayLog in dayLogs)
            {
                if (dayLog.LogEntries.Count > 0)
                    report = LogProcessor.GetReport(dayLog.LogEntries);
                else
                    report = new Report { ReportDate = dayLog.Date, Title = Policy.ReportTitle };
                report.ReportDate = dayLog.Date;
                reports.Add(report);
            }
            reports = reports.OrderBy(r => r.ReportDate).ToList();
            return reports;
        }

        protected virtual void AddPullRequests(Report report, Log log)
        {
            report.PullRequests = null;
        }
        protected List<DayLog> GetDayLogs(Log log, List<DateTime> dates)
        {
            var dayLogs = new List<DayLog>();
            foreach (var date in dates)
                dayLogs.Add(new DayLog { Date = date, LogEntries = log.Entries.FindAll(e => e.Date >= date && e.Date < date.AddDays(1)) });
            return dayLogs;
        }

        public abstract Log CreateLog();
    }
}
