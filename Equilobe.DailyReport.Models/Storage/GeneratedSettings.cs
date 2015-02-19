using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class GeneratedSettings
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }
        public DateTime LastFinalReportSentDate { get; set; }
        public DateTime LastDraftSentDate { get; set; }
        public bool WasResetToDefaultToday { get; set; } // reset procedure may not be used anymore
        public DateTime LastFinalDraftConfirmationDate { get; set; } // if we first check this property when confirming individual drafts, it cannot be changed by more than one instance at a time;
                                                                   // is set in the web app;
        public bool WasForcedByLead { get; set; } // TODO: remove this property. it can be checked by verifying LastDraftSentDate == DateTime.Today
        public List<IndividualDraftSettings> IndividualDrafts { get; set; }
    }
}
