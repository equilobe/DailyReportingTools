using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.JiraOriginals
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
