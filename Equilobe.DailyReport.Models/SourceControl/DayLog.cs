using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models
{
    public class DayLog
    {
        public DateTime Date { get; set; }
        public List<LogEntry> LogEntries { get; set; }
    }
}
