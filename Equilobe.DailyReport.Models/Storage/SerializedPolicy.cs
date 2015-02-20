using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class SerializedPolicy
    {
        public long Id { get; set; }
        public long ReportSettingsId { get; set; }
        public string PolicyString { get; set; }
    }
}
