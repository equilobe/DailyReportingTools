using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Policy
{
    public class User
    {
        public string JiraUserKey { get; set; }
        public string JiraDisplayName { get; set; }
        public List<string> SourceControlUsernames { get; set; }
        public string EmailOverride { get; set; }
        public string EmailAdress { get; set; }

        [XmlAttribute]
        public bool Included { get; set; }

        public User()
        {
            SourceControlUsernames = new List<string>();
            Included = true;
        }
    }
}
