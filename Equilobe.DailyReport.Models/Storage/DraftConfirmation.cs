using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class DraftConfirmation
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }
        public DateTime? LastFinalDraftConfirmationDate { get; set; }
    }
}
