using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Storage
{
    public class IndividualDraftInfo
    {
        public string Username { get; set; }
        public string UserKey { get; set; }
        public DateTime LastConfirmationDate { get; set; }
        public bool IsLead { get; set; }
        [XmlIgnore]
        public Uri ConfirmationDraftUrl { get; set; }
        [XmlIgnore]
        public Uri ResendDraftUrl { get; set; }
        [XmlIgnore]
        public Uri ForceDraftUrl { get; set; }
    }
}
