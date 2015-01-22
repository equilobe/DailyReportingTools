using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    public class IndividualDraftInfo
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string UserKey { get; set; }
        public bool Confirmed { get; set; }
        [XmlIgnore]
        public Uri ConfirmationDraftUrl { get; set; }
        [XmlIgnore]
        public Uri ResendDraftUrl { get; set; }
    }
}
