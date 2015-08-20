using Equilobe.DailyReport.Models.PayPal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class IPNLog
    {
        public long Id { get; set; }
        public string SerializedPayPalInfo { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? DateProcessed { get; set; }
    }
}
