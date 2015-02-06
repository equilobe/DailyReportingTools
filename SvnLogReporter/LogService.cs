using Equilobe.DailyReport.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter
{
   public static class LogService
    {
        public static void RemoveWrongEntries(DateTime fromDate, Log log)
        {
            if (!log.Entries.Any())
                return;

            if (log.Entries.First().Date < fromDate)
                log.Entries.Remove(log.Entries.First());

            log.Entries = log.Entries
                         .Where(e => e.Author != null)
                         .Where(e => e.Date != default(DateTime))
                         .ToList();
        }

        public static Log LoadLog(string filePath)
        {
            XmlSerializer xs = new XmlSerializer(typeof(Log));
            using (var fs = File.OpenRead(filePath))
            {
                var log = (Log)xs.Deserialize(fs);
                return log;
            }
        }
    }
}
