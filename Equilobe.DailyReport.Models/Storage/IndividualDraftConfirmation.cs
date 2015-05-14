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
        public long BasicSettingsId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string UniqueUserKey { get; set; }
        public DateTime? LastDateConfirmed { get; set; }
        //ReportDate represents the day of the report (rounded to 00:00) and is converted to the JIRA timezone
        public string ReportDate { get; set; }
        public bool IsProjectLead { get; set; }

        public virtual BasicSettings BasicSettings { get; set; }
    }
}
