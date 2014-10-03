using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace SvnLogReporter.Model
{
    public class LogEntry
    {
        [XmlElement("author")]
        public string Author { get; set; }
        [XmlElement("date")]
        public DateTime Date { get; set; }
        [XmlElement("msg")]
        public string Message { get; set; }
        [XmlAttribute("revision")]
        public string Revision { get; set; }
        [XmlIgnore]
        public string Link { get; set; }
        [XmlIgnore]
        public List<Octokit.PullRequest> PullRequests { get; set; }
    }
}
