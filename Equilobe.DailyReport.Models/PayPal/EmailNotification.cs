using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.PayPal
{
    public class EmailNotification
    {
        public string InstanceUrl { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string ExpirationDateString { get; set; }
        public string SubscriptionId { get; set; }
    }
}
