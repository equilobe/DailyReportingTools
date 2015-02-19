using Equilobe.DailyReport.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class ReportResendScope 
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }
        public ResendScope Scope { get; set; }
        public string UniqueUserKey { get; set; }
        public DateTime Date { get; set; }
    }
}
