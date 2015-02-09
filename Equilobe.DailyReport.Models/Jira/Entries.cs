using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [Serializable]
    public class Entries
    {
        [XmlElement("comment")]
        public string Comment { get; set; }

        [XmlElement("timeSpent")]
        public int TimeSpent { get; set; }

        [XmlElement("author")]
        public string Author { get; set; }

        [XmlElement("authorFullName")]
        public string AuthorFullName { get; set; }

        [XmlElement("created")]
        public DateTime Created { get; set; }

        [XmlElement("startDate")]
        public DateTime StartDate { get; set; }

        [XmlElement("updateAuthor")]
        public string UpdateAuthor { get; set; }

        [XmlElement("updateAuthorFullName")]
        public string UpdateAuthorFullName { get; set; }

        [XmlElement("updated")]
        public DateTime Updated { get; set; }
    }
}
