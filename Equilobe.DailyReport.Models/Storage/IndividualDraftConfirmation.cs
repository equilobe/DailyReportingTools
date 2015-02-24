using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class IndividualDraftConfirmation
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string UniqueUserKey { get; set; }
        public DateTime? LastDateConfirmed { get; set; }
        public bool IsProjectLead { get; set; }

        public virtual ReportSettings ReportSettings { get; set; }
    }
}
