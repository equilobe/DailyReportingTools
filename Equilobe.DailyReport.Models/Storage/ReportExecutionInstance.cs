﻿using Equilobe.DailyReport.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportExecutionInstance 
    {
        public long Id { get; set; }
        public long BasicSettingsId { get; set; }
        public SendScope Scope { get; set; }
        public string UniqueUserKey { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateExecuted { get; set; }
        public string Status { get; set; }

        public virtual BasicSettings BasicSettings { get; set; }
    }
}
