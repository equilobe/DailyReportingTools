using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class SubscriptionDetails
    {

        // TODO: 
        // Add subscription related info to the InstalledInstance entity
        // Create new table for tranzaction related info

        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public string SubscriptionId { get; set; }
        public string TxnId { get; set; } 
        public string TxnType { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double PaymentGross { get; set; }

        public DateTime TrialStartDate { get; set; }
        public DateTime TrialEndDate { get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
    }
}
