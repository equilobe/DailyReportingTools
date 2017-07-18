using Equilobe.DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL
{
    class LogHelpers
    {
        public static void RemoveWrongEntries(DateTime fromDate, Log log)
        {
            if (log == null || log.Entries.IsEmpty())
                return;

            if (log.Entries.First() != null && log.Entries.First().Date < fromDate)
                log.Entries.Remove(log.Entries.First());

            log.Entries = log.Entries
                         .Where(e => e!= null && e.Author != null && e.Date != default(DateTime))
                         .ToList();
        }
    }
}
