using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class EntryContext
    {
        public IssueDetailed Issue { get; set; }
        public string AuthorName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan OffsetFromUtc { get; set; }
    }
}
