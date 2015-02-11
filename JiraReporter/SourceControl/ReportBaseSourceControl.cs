using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira;

namespace JiraReporter.SourceControl
{
    public abstract class ReportBaseSourceControl
    {
        protected JiraPolicy Policy { get; private set; }
        protected JiraOptions Options { get; private set; }

        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Policy.GeneratedProperties.LogPath, Options.ReportDate);
            }
        }

        public string GetLogFilePath(string logPath, DateTime reportDate)
        {
            return Path.Combine(logPath, reportDate.ToString("yyyy-MM-dd") + ".xml");
        }

        public ReportBaseSourceControl(JiraPolicy policy, JiraOptions options)
        {
            Policy = policy;
            Options = options;
        }

        public static T Create<T>(JiraPolicy p, JiraOptions o) where T : ReportBase
        {
            return Activator.CreateInstance(typeof(T), p, o) as T;
        }
    }
}
