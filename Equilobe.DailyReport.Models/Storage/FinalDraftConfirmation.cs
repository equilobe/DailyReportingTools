﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class FinalDraftConfirmation
    {
        [Key]
        public long BasicSettingsId { get; set; }
        public DateTime? LastFinalDraftConfirmationDate { get; set; }

        public virtual BasicSettings BasicSettings { get; set; }
    }
}
