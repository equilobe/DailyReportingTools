using Equilobe.DailyReport.Models.PayPal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.PayPal
{
    public class SubscriptionContext
    {
        public string Username { get; set; }
        public string BaseUrl { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public string SubscriptionId { get; set; }
        public string SubscriptionPeriod { get; set; }
    }
}
