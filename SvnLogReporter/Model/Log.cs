using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SvnLogReporter.Model
{
    [XmlRoot("log")]
    public class Log
    {
        [XmlElement("logentry")]
        public List<LogEntry> Entries { get; set; }

        [XmlIgnore]
        public List<Octokit.PullRequest> PullRequests { get; set; }

        public void RemoveWrongEntries(DateTime fromDate)
        {
            if (!Entries.Any())
                return;

            if (Entries.First().Date < fromDate)
                Entries.Remove(Entries.First());

            Entries = Entries
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
