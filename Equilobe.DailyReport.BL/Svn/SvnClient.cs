using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL.Svn
{
    public class SvnClient
    {
        ISourceControlContext Context { get; set; }

        public SvnClient(ISourceControlContext context)
        {
            Context = context;
        }

        private string GetCommandString(bool full)
        {
            var command = new StringBuilder();
            command.AppendFormat("svn log {0} --xml", Context.SourceControlOptions.Repo);

            if (!full)
                command.Append(" --quiet");

            if (!string.IsNullOrEmpty(Context.SourceControlOptions.Credentials.Username) && !string.IsNullOrEmpty(Context.SourceControlOptions.Credentials.Password))
                command.AppendFormat(" --username \"{0}\" --password \"{1}\"",
                                     Context.SourceControlOptions.Credentials.Username,
                                     Context.SourceControlOptions.Credentials.Password);

            command.AppendFormat(" -r{{{0:yyyy-MM-ddTHH:mmZ}}}:{{{1:yyyy-MM-ddTHH:mmZ}}}",
                                 Context.FromDate,
                                 Context.ToDate);

            return command.ToString();
        }

        public void SetCommitsLink(List<LogEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                if (entry.Link == null && Context.SourceControlOptions.CommitUrl != null)
                    entry.Link = Context.SourceControlOptions.CommitUrl + entry.Revision;
        }

        /// <param name="full">enables providing full informations for log with slightly increased performance hit</param>
        public Log GetLog(bool full = false)
        {
            var command = GetCommandString(full);
            var xmlLogOutput = Cmd.Execute(command);

            var log = Deserialization.XmlDeserialize<Log>(xmlLogOutput);
            LogHelpers.RemoveWrongEntries(Context.FromDate, log);

            if (full)
                SetCommitsLink(log.Entries);

            return log;
        }

        public Log GetLogWithCommitLinks()
        {
            return GetLog(true);
        }

        public List<string> GetAllAuthors()
        {
            return GetLog().Entries.Select(logEntry => logEntry.Author)
                                   .Distinct()
                                   .ToList();
        }
    }
}
