﻿using RazorEngine;
using RazorEngine.Templating;
using SourceControlLogReporter.Model;
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

        private string GetCommandString()
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
