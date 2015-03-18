﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class SerializedPolicy
    {
        [Key]
        public long ReportSettingsId { get; set; }
        public string PolicyString { get; set; }

        public virtual ReportSettings ReportSettings { get; set; }
    }
}
