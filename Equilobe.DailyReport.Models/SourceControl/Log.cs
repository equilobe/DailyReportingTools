using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models
{
    [XmlRoot("log")]
    public class Log
    {
        [XmlElement("logentry")]
        public List<LogEntry> Entries { get; set; }

        [XmlIgnore]
        public List<PullRequest> PullRequests { get; set; }
    }
}
