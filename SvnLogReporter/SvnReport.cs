using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.ReportPolicy;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter
{
    public class SvnReport : ReportBase
    {
        public SvnReport(Policy p, Options o):base(p,o)
        {

        }

        public SvnReport()
        {

        }

        public override Log CreateLog()
        {
            ExecuteSvnCommand();
            var log= LogService.LoadLog(PathToLog);
            LogService.RemoveWrongEntries(Options.FromDate, log);
            SetCommitsLink(log.Entries);
            return log;
        }

        public virtual void SetCommitsLink(List<LogEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                if (entry.Link == null && Policy.SourceControlOptions.CommitUrl != null)
                    entry.Link = Policy.SourceControlOptions.CommitUrl + entry.Revision;
        }

        public virtual void ExecuteSvnCommand()
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

        public string GetCommandString()
        {
            return string.Format("svn log {0} --xml --username \"{1}\" --password \"{2}\" -r{{{3:yyyy-MM-ddTHH:mmZ}}}:{{{4:yyyy-MM-ddTHH:mmZ}}} > \"{5}\"",
                            Policy.SourceControlOptions.RepoUrl,
                            Policy.SourceControlOptions.Username,
                            Policy.SourceControlOptions.Password,
                            Options.FromDate,
                            Options.ToDate,
                            PathToLog);
        }


    }
}
