using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.ReportPolicy
{
    public class JiraGeneratedProperties : GeneratedProperties
    {
        public DateTime LastReportSentDate { get; set; }
        public DateTime LastDraftSentDate { get; set; }
        public bool WasResetToDefaultToday { get; set; }
        public string UniqueProjectKey { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public List<IndividualDraftInfo> IndividualDrafts { get; set; }
        [XmlIgnore]
        public bool IsFinalDraft { get; set; }
        [XmlIgnore]
        public bool IsIndividualDraft { get; set; }
        [XmlIgnore]
        public bool IsFinalReport { get; set; }
        public bool IsFinalDraftConfirmed { get; set; }
        public bool WasForcedByLead { get; set; }

        [XmlIgnore]
        public string ProjectManager { get; set; }
    }
}
