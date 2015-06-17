using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class SubscriptionDetails
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }

        public string SubscriptionId { get; set; }
        public string TxnType { get; set; }
        public string SubscriptionDate { get; set; }
        public string PaymentDate { get; set; }
        public double PaymentGross { get; set; }

        
    }
}
