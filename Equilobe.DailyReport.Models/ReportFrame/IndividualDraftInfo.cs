using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class IndividualDraftInfo
    {
        public string Username { get; set; }
        public string UniqueUserKey { get; set; }
        public DateTime? LastConfirmationDate { get; set; }

        //ReportDate represents the day of the report (rounded to 00:00) and is converted to the JIRA timezone
        public string ReportDate { get; set; }
        public bool IsProjectLead { get; set; }
        public Uri ConfirmationDraftUrl { get; set; }
        public Uri ResendDraftUrl { get; set; }
        public Uri ForceDraftUrl { get; set; }
    }
}
