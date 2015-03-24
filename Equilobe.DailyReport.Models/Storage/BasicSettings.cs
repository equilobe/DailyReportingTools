using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class BasicSettings : IBasicSettings
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        [Required]
        public long ProjectId { get; set; }
        public string ReportTime { get; set; }
        [Required]
        public string UniqueProjectKey { get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
        public virtual ReportExecutionSummary ReportExecutionSummary { get; set; }
        public virtual SerializedAdvancedSettings SerializedAdvancedSettings { get; set; }
        public virtual FinalDraftConfirmation FinalDraftConfirmation { get; set; }

        public virtual ICollection<IndividualDraftConfirmation> IndividualDraftConfirmations { get; set; }
        public virtual ICollection<ReportExecutionInstance> ReportExecutionInstances { get; set; }

    }
}
