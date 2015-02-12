using Equilobe.DailyReport.Models.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [Serializable]
    public class JiraIssueSmall
    {
        [XmlElement("key")]
        public string Key { get; set; }      

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("entries", Type = typeof(Entries))]
        public List<Entries> Entries { get; set; }
    }
}
