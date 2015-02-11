using CommandLine;
using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraOptions : Equilobe.DailyReport.Models.SourceControl.Options
    {
        [Option(null, "draftKey", Required = false)]
        public string DraftKey { get; set; }
        [Option(null, "triggerKey", Required = false)]
        public string TriggerKey { get; set; }

        public JiraPolicy Policy { get; set; }
    }
}
