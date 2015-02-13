using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class GeneratedSettings
    {
        public long ReportSettingsId { get; set; }
        public DateTime LastReportSentDate { get; set; }
        public DateTime LastDraftSentDate { get; set; }
        public bool WasResetToDefaultToday { get; set; }
        public bool IsFinalDraftConfirmed { get; set; }
        public bool WasForcedByLead { get; set; }
        public List<IndividualDraftSettings> IndividualDrafts { get; set; }
    }
}
