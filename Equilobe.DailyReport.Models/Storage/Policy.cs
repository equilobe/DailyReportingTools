using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Storage
{
    public class Policy
    {
        public string ReportTime { get; set; }
        public string ReportTitle { get; set; }

        [XmlIgnore]
        public DateTime ReportTimeDateFormat { get; set; }

        public string Emails { get; set; }

        public SourceControlOptions SourceControlOptions { get; set; }

        public GeneratedProperties GeneratedProperties { get; set; }

        [XmlIgnore]
        public List<string> EmailCollection { get; set; }
    }
}
