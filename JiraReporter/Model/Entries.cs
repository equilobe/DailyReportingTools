using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JiraReporter.Model
{
    [Serializable]
    public class Entries
    {
        [XmlElement("comment")]
        public string Comment;

        [XmlElement("timeSpent")]
        public string TimeSpent;

        [XmlElement("author")]
        public string Author;

        [XmlElement("authorFullName")]
        public string AuthorFullName;

        [XmlElement("created")]
        public DateTime Created;

        [XmlElement("startDate")]
        public DateTime StartDate;

        [XmlElement("updateAuthor")]
        public string UpdateAuthor;

        [XmlElement("updateAuthorFullName")]
        public string UpdateAuthorFullName;

        [XmlElement("updated")]
        public DateTime Updated;
    }
}
