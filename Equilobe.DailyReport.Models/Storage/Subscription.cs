using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Storage
{
    public class Subscription
    {
        public string Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public DateTime SubscriptionDate { get; set; }
        public DateTime? TrialEndDate { get; set; }

        public virtual InstalledInstance InstalledInstance { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}
