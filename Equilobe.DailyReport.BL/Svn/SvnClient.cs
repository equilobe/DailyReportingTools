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
        SourceControlOptions SourceControlOptions { get { return Context.SourceControlOptions; } }
        DateTime FromDate { get { return Context.FromDate; } }
        DateTime ToDate { get { return Context.ToDate; } }

        public SvnClient(ISourceControlContext context)
        {
            Context = context;
        }

        void SetCommitsLink(List<LogEntry> entries)
        {
            if (entries == null)
                return;

            foreach (var entry in entries)
                if (entry.Link == null && SourceControlOptions.CommitUrl != null)
                    entry.Link = SourceControlOptions.CommitUrl + entry.Revision;
        }

        void ExecuteSvnCommand(string pathToLog)
        {
            Validations.EnsureDirectoryExists(Path.GetDirectoryName(pathToLog));
            try
            {
                var command = GetCommandString(pathToLog);
                Cmd.ExecuteSingleCommand(command);
            }
            catch (Exception ex)
            {
                throw new SvnNotAvailableException(ex);
            }
        }

        public Log GetLog(string pathToLog)
        {
            ExecuteSvnCommand(pathToLog);
            var log = Deserialization.XmlDeserializeFileContent<Log>(pathToLog);
            LogHelpers.RemoveWrongEntries(FromDate, log);
            SetCommitsLink(log.Entries);

            return log;
        }

        string GetCommandString(string pathToLog)
        {
            return string.Format("svn log {0} --xml --username \"{1}\" --password \"{2}\" -r{{{3:yyyy-MM-ddTHH:mmZ}}}:{{{4:yyyy-MM-ddTHH:mmZ}}} > \"{5}\"",
                            SourceControlOptions.Repo,
                            SourceControlOptions.Credentials.Username,
                            SourceControlOptions.Credentials.Password,
                            FromDate,
                            ToDate,
                            pathToLog);
        }
    }
}
