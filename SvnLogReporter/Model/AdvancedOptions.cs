using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SourceControlLogReporter.Model
{
    
    public class AdvancedOptions
    {
        [XmlAttribute]
        public bool NoDraft { get; set; }
        [XmlAttribute]
        public bool NoIndividualDraft { get; set; }
        public string ReportTitle { get; set; }
    }
}
