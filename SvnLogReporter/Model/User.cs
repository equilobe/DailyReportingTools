using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    public class User
    {
        public string JiraUserKey { get; set; } // use jira username instead of full name
        public List<string> SourceControlUsernames { get; set; } // add list of usernames in case of multiple accounts

        [XmlAttribute]
        public bool Ignored { get; set; }
    }
}
