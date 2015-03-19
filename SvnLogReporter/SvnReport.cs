using Equilobe.DailyReport.Models;
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
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Interfaces;

namespace SourceControlLogReporter
{
    public class SvnReport : ReportBase
    {
        public ISvnService SvnService { get; set; }

        public SvnReport(Policy p, Options o):base(p,o)
        {

        }

        public SvnReport()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext{SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate};
            var log = SvnService.GetLog(context, PathToLog);
            return log;
        }
    }
}
