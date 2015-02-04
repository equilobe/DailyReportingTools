using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using SourceControlLogReporter.Model;
using System.Globalization;

namespace SourceControlLogReporter
{
    public class Options
    {
        [Option(null, "policy", Required = true)]
        public string PolicyPath { get; set; }
        [Option(null, "to", Required = false)]
        public string StringToDate { get; set; }
        [Option(null, "from", Required = false)]
        public string StringFromDate { get; set; }
        [Option(null, "noemail", Required = false, HelpText = "Don't email report")]
        public bool NoEmail { get; set; }
        [Option(null, "draftKey", Required = false)]
        public string DraftKey { get; set; }
        [Option(null, "triggerKey", Required = false)]
        public string TriggerKey { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<DateTime> ReportDates { get; set; }

        public bool HasToDate { get; set; }
        public bool HasFromDate { get; set; }
    }
}
