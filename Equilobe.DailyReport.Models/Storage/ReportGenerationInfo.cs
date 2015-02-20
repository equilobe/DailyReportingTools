﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportGenerationInfo
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }
        public DateTime? LastFinalReportSentDate { get; set; }
        public DateTime? LastDraftSentDate { get; set; }
        public List<IndividualDraftLog>? IndividualDrafts { get; set; }
    }
}