using CommandLine;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraOptions : Equilobe.DailyReport.Models.SourceControl.Options
    {
        [Option(null, "uniqueProjectKey", Required = true)]
        public string UniqueProjectKey { get; set; }

        public JiraPolicy Policy { get; set; }
    }
}
