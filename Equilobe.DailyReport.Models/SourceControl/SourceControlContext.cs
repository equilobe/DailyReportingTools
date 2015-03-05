using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System;

namespace Equilobe.DailyReport.Models.SourceControl
{
    public class SourceControlContext : ISourceControlContext
    {
        public SourceControlOptions SourceControlOptions { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
