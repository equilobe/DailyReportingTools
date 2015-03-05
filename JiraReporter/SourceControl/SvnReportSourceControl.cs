using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Storage;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Policy;

namespace JiraReporter.SourceControl
{
    class SvnReportSourceControl : SvnReport
    {
        JiraReport Context { get; set; }
        JiraPolicy Policy { get { return Context.Policy; } }
        JiraOptions Options { get { return Context.Options; } }
        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Context.LogPath, Options.ReportDate);
            }
        }

        public SvnReportSourceControl(JiraReport context)
        {
            Context = context;
        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext{SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate};
            var log = new SvnService().GetLog(context, PathToLog);
            return log;
        }
    }
}
