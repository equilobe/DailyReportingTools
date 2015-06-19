using Equilobe.DailyReport.Models.Paypal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class SubscriptionContext
    {
        public string Username { get; set; }
        public string BaseUrl { get; set; }
        public double PaymentGross { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime TrialStartDate { get; set; }
        public DateTime TrialEndDate { get; set; }
        public string SubscriptionId { get; set; }
    }
}
