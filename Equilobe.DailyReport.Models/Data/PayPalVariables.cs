using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Data
{
    public class PayPalVariables
    {
        public static string SubscriptionCanceled = "subscr_cancel";
        public static string SubscriptionExpired = "subscr_eot";
        public static string SubscriptionFailed = "subscr_failed";
        public static string SubscriptionModify = "subscr_modify";
        public static string SubscriptionPayment = "subscr_payment";
        public static string SubscriptionSignup = "subscr_signup";

        public static string PaymentCompleted = "Completed";
        public static string PaymentPending = "Pending";
        public static string PaymentDenied = "Denied";
    }
}
