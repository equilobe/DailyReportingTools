using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceControlLogReporter.Model;
using System.Xml.Serialization;
using System.IO;
using RazorEngine;
using System.Security;


namespace SourceControlLogReporter
{
    public class LogProcessor
    {
        //private static string DoubleNewLine = Environment.NewLine + Environment.NewLine;
        //private static string DateFormat = "dd.MM.yyy HH:mm";        

        public static Report GetReport(List<LogEntry> log)
        {
            return new Report
            {
                Authors = log.GroupBy(e => e.Author, GetAuthor),
            };
        }

        private static Author GetAuthor(string name, IEnumerable<LogEntry> entries)
        {
            return new Author
            {
                Name = name,
                Entries = entries.Select(GetEntryLines)
                                      .GroupBy(e=>e.Message, GetEntry),
                EntryCount = entries.Count()
            };
        }
        private static LogEntry GetEntry(string message, IEnumerable<LogEntry> entries)
        {
            message = entries.First().Message;
            if (message == NoLogMessage)
                message += " (" + entries.Count() + " entries)";
            return new LogEntry
            {
                Message = message
            };
        }

        private static LogEntry GetEntryLines(LogEntry entry)
        {
            var messageLines = GetNonEmptyTrimmedLines(entry.Message);
            return new LogEntry
            {
                Message=messageLines ?? NoLogMessage
            };
        }

        static readonly string NoLogMessage = "*** NO LOG MESSAGE";

        public static string GetNonEmptyTrimmedLines(string message)
        {
            message = message ?? string.Empty;
            message = SecurityElement.Escape(message);
            message = message.Replace("\n", "<br />");
            message = message.Replace("<br /><br />", "<br />");
            message = message.Trim();
            message = message.RemoveSpace();
            return message;
        }

    }
}
