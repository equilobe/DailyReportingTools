using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    public class GeneratedProperties
    {
        public DateTime LastReportSentDate { get; set; }
        public string UniqueProjectKey { get; set; }
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
