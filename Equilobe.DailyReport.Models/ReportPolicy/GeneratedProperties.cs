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
    public class GeneratedProperties
    {
        public DateTime LastReportSentDate { get; set; }
        public DateTime LastDraftSentDate { get; set; }
        public bool WasResetToDefaultToday { get; set; }
        public string UniqueProjectKey { get; set; }
        public string RootPath { get; set; }
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

        [XmlIgnore]
        public string LogPath { get { return Path.Combine(RootPath, "Logs"); } }
        [XmlIgnore]
        public string LogArchivePath { get { return Path.Combine(RootPath, "LogArchive"); } }
        [XmlIgnore]
        public string ReportsPath { get { return Path.Combine(RootPath, "Reports"); } }
        [XmlIgnore]
        public string UnsentReportsPath { get { return Path.Combine(RootPath, "UnsentReports"); } }
    }
}
