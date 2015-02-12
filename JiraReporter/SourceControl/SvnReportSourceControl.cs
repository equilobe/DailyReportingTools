using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;

namespace JiraReporter.SourceControl
{
    class SvnReportSourceControl : SvnReport
    {
        JiraPolicy Policy { get; set; }
        JiraOptions Options { get; set; }
        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Policy.GeneratedProperties.LogPath, Options.ReportDate);
            }
        }

        public SvnReportSourceControl(JiraPolicy policy, JiraOptions options)
        {
            Policy = policy;
            Options = options;
        }

        public override Log CreateLog()
        {
            ExecuteSvnCommand();
            var log = LogService.LoadLog(PathToLog);
            LogService.RemoveWrongEntries(Options.FromDate, log);
            SetCommitsLink(log.Entries);
            return log;
        }

        public override void ExecuteSvnCommand()
        {
            Validation.EnsureDirectoryExists(Path.GetDirectoryName(PathToLog));
            try
            {
                var command = GetCommandString();
                Cmd.ExecuteSingleCommand(command);
            }
            catch (Exception)
            {
                return;
            }
        }

        public override void SetCommitsLink(List<LogEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                if (entry.Link == null && Policy.SourceControlOptions.CommitUrl != null)
                    entry.Link = Policy.SourceControlOptions.CommitUrl + entry.Revision;
        }
    }
}
