using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Policy
{
    public class Policy
    {
        public string ReportTime { get; set; }
        public string ReportTitle { get; set; }

        [XmlIgnore]
        public DateTime ReportTimeDateFormat { get; set; }

        public string Emails { get; set; }

        public SourceControlOptions SourceControlOptions { get; set; }

        [XmlIgnore]
        public List<string> EmailCollection { get; set; }

        public string RootPath { get; set; }

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
