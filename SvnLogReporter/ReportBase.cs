using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter
{
    abstract class ReportBase
    {
        protected Policy Policy { get; private set; }
        protected Options Options { get; private set; }

        public string PathToLog
        {
            get
            {
                return GetLogFilePath(Policy.LogPath, Options.ReportDate);
            }
        }

        public string GetLogFilePath(string logPath,DateTime reportDate)
        {
            return Path.Combine(logPath, reportDate.ToString("yyyy-MM-dd") + ".xml");
        }

        protected ReportBase(Policy p, Options o)
        {
           Policy = p;
           Options = o;
        }

        public static T Create<T>(Policy p, Options o) where T : ReportBase
        {
            return Activator.CreateInstance(typeof(T), p, o) as T;
        }

        public virtual string TryGenerateReport()
        {
            try
            {
               return GenerateReport();
            }
            catch (SvnNotAvailableException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string GenerateReport()
        {            
            var log = CreateLog();
            return GetReportContent(log);                      
        }

        protected string GetReportContent(Log log)
        {
            var reportContent = "";
            var reports = GetReports(log);
                foreach (var report in reports)
                        reportContent += ProcessReport(Policy, report);         
            return reportContent;
        }
        protected abstract string ProcessReport(Policy p, Report report);       

        protected abstract List<Report> GetReports(Log log);
        
        protected List<Report> EmptyReports (Dictionary<DateTime, List<LogEntry>> logs, List<DateTime> dates)
        {
            List<Report> reports = new List<Report>();
            foreach (var date in dates)
                if (!logs.ContainsKey(date))
                    reports.Add(new Report() { ReportDate = date, Title=Policy.ReportTitle});
            return reports;
        }

        protected Dictionary<DateTime, List<LogEntry>> GetDayLogs (Log log)
    {
        var logs = new Dictionary<DateTime, List<LogEntry>>();
        var date = new DateTime();
        foreach (var entry in log.Entries)
        {
            date = Options.FloorToDay(entry.Date);
            Add(logs, date, entry);
        }
        logs = logs.OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Value);
        return logs;
    }

        protected void Add(Dictionary<DateTime, List<LogEntry>> dict, DateTime key, LogEntry value)
        {
            if (dict.ContainsKey(key))
            {
                List<LogEntry> list = dict[key];
                if (list.Contains(value) == false)
                {
                    list.Add(value);
                }
            }
            else
            {
                List<LogEntry> list = new List<LogEntry>();
                list.Add(value);
                dict.Add(key, list);
            }
        }

        protected abstract Log CreateLog();        
    }
}
