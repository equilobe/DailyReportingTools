using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.PayPal
{
    public class PayPalLog
    {
        public PayPalCheckoutInfo PayPalInfo { get; set; }
        public DateTime Date { get; set; }
    }
}
