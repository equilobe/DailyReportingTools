using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.Policy
{
    public class AdvancedOptions 
    {
        public string ReportTitle { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool NoDraft { get; set; }

        [XmlAttribute]
        [DefaultValue(false)]
        public bool NoIndividualDraft { get; set; }

        [DefaultValue(false)]
        public bool SendDraftToAllUsers { get; set; }

        [DefaultValue(true)]
        public bool SendDraftToProjectManager { get; set; }

        [DefaultValue(false)]
        public bool SendDraftToOthers { get; set; }

        [DefaultValue(true)]
        public bool SendFinalToAllUsers { get; set; }

        [DefaultValue(false)]
        public bool SendFinalToOthers { get; set; }

        [DefaultValue("permanent")]
        public string PermanentTaskLabel { get; set; }

        [DefaultValue("Reopened")]
        public string ReopenedStatus { get; set; }

        public List<string> AdditionalWorkflowStatuses { get; set; }

        public List<DayOfWeek> WeekendDaysList { get; set; }

        public AdvancedOptions()
        {
            PermanentTaskLabel = "permanent";
            ReopenedStatus = "Reopened";
            SendDraftToProjectManager = true;
            SendFinalToAllUsers = true;
            NoIndividualDraft = false;
            NoDraft = false;
        }
    }
}
