﻿using Equilobe.DailyReport.Models.Storage;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;
using Autofac;

namespace JiraReporter.SourceControl
{
    public abstract class ReportBaseSourceControl
    {
        //protected JiraPolicy Policy { get; private set; }
        //protected JiraOptions Options { get; private set; }
        protected JiraReport Report { get; set; }

        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Report.LogPath, Report.Options.ReportDate);
            }
        }

        public string GetLogFilePath(string logPath, DateTime reportDate)
        {
            return Path.Combine(logPath, reportDate.ToString("yyyy-MM-dd") + ".xml");
        }

        public ReportBaseSourceControl(JiraReport report)
        {
            Report = report;
        }

        public static T Create<T>(JiraReport report) where T : ReportBase
        {
            var sourceControlProcessor = DependencyInjection.Container.Resolve<T>(new TypedParameter(typeof(JiraReport), report));

            return sourceControlProcessor;
        }
    }
}
