using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.PayPal
{
    public class PaymentContext
    {
        public DateTime PaymentDate { get; set; }
        public double Gross { get; set; }
        public double Fee { get; set; }
        public string TransactionId { get; set; }
        public string ParentTransactionId { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string SubscriptionId { get; set; }
        public string Currency { get; set; }
    }
}
