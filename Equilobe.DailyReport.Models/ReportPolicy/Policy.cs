using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
namespace Equilobe.DailyReport.Models.ReportPolicy
{
    public class Policy
    {
        public string ReportTime { get; set; }

        [XmlIgnore]
        public DateTime ReportTimeDateFormat { get; set; }

        public string Emails { get; set; }

        public SourceControlOptions SourceControlOptions { get; set; }

        public GeneratedProperties GeneratedProperties { get; set; }

        [XmlIgnore]
        public List<string> EmailCollection { get; set; }
    }
}
