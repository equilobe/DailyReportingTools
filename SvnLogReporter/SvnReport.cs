﻿using RazorEngine;
using RazorEngine.Templating;
using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter
{
    public class SvnReport : ReportBase
    {
        public SvnReport(Policy p, Options o):base(p,o)
        {

        }
        public override Log CreateLog()
        {
            ExecuteSvnCommand();
            var log= Log.LoadLog(PathToLog);
            log.RemoveWrongEntries(Options.FromDate);
            return log;
        }

        private void ExecuteSvnCommand()
        {
            Validation.EnsureDirectoryExists(Path.GetDirectoryName(PathToLog));
            try
            {
                var command = GetCommandString();
                Cmd.ExecuteSingleCommand(command);
            }
            catch (Exception ex)
            {
                throw new SvnNotAvailableException(ex);
            }
        }

        protected override List<Report> GetReports(Log log)
        {
            var reports = new List<Report>();
            var logs = GetDayLogs(log);
            var report = new Report();
            var dates = new List<DateTime>();
            Options.GetDates(dates);
            reports = EmptyReports(logs, dates);
            foreach (var logDict in logs)
            {
                report = LogProcessor.GetReport(logDict.Value);
                report.ReportDate = logDict.Key;
                reports.Add(report);
                report.PullRequests = null;
            }
            reports = reports.OrderBy(r => r.ReportDate).ToList();
            return reports;
        }      

        private string GetCommandString()
        {
            return string.Format("svn log {0} --xml --username \"{1}\" --password \"{2}\" -r{{{3:yyyy-MM-ddTHH:mmZ}}}:{{{4:yyyy-MM-ddTHH:mmZ}}} > \"{5}\"",
                            Policy.SourceControl.RepoUrl,
                            Policy.SourceControl.Username,
                            Policy.SourceControl.Password,
                            Options.FromDate,
                            Options.ToDate,
                            PathToLog);
        }


    }
}
