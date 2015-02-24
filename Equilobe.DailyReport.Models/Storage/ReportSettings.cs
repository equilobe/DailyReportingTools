﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportSettings
    {
        public long Id { get; set; }
        [Required]
        public string BaseUrl { get; set; }
        public long ProjectId { get; set; }

        [Required]
        public string ReportTime { get; set; }
        [Required]
        public string UniqueProjectKey { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public virtual ReportExecutionSummary ReportExecutionSummary { get; set; }
        public virtual SerializedPolicy SerializedPolicy { get; set; }
        public virtual FinalDraftConfirmation FinalDraftConfirmation { get; set; }

        public virtual ICollection<IndividualDraftConfirmation> IndividualDraftConfirmations { get; set; }
        public virtual ICollection<ReportExecutionInstance> ReportExecutionInstances { get; set; }

    }
}
