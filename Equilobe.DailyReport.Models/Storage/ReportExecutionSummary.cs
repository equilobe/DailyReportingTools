﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportExecutionSummary
    {
        [Key]
        public long BasicSettingsId { get; set; }
        public DateTime? LastFinalReportSentDate { get; set; }
        public DateTime? LastDraftSentDate { get; set; }

        public virtual BasicSettings BasicSettings { get; set; }
        
    }
}
