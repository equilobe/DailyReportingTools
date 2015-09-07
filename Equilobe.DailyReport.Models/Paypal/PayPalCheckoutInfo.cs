using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.PayPal
{
    public class PayPalCheckoutInfo
    {
        #region "Transaction and Notification-Related Variables"
        public string receiver_email { get; set; } 
        public string receiver_id { get; set; } 
        public string txn_id { get; set; }
        public string parent_txn_id { get; set; }
        public string txn_type { get; set; }
        public string verify_sign { get; set; }
        #endregion

        #region "Buyer Information Variables"
        public string address_country { get; set; } 
        public string address_city { get; set; } 
        public string address_country_code { get; set; }  
        public string address_name { get; set; } 
        public string address_state { get; set; } 
        public string address_status { get; set; }  
        public string address_street { get; set; } 
        public string address_zip { get; set; } 
        public string contact_phone { get; set; } 
        public string first_name { get; set; } 
        public string last_name { get; set; } 
        public string payer_email { get; set; }
        public string payer_id { get; set; } 
        public int? Zip
        {
            get
            {
                int temp;
                if (int.TryParse(address_zip, out temp))
                {
                    return temp;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion
        #region "Payment Information Variables"
        public string token { get; set; }    
        public string invoice { get; set; } 
        public string item_name { get; set; } 
        public string item_number { get; set; }
        public string mc_currency { get; set; } 
        public string mc_fee { get; set; } 
        public string mc_gross { get; set; }          
        public string payer_status { get; set; }
        public string payment_date { get; set; }
        public string retry_at { get; set; }

        public DateTime TrxnDate
        {
            get
            {
                DateTime dt = DateTime.Now;
                if (DateTime.TryParse(payment_date, out dt))
                {
                    return dt;
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }
    
        public string payment_status { get; set; }     
        public string payment_type { get; set; }
        public string pending_reason { get; set; }
        public string protection_eligibility { get; set; }
        public string quantity { get; set; }
        public string reason_code { get; set; }
        public string correlationID { get; set; }
        public string ack { get; set; }
        public string errmsg { get; set; }
        public int? errcode { get; set; }

        public string custom { get; set; }
        public string period1 { get; set; }
        public string period2 { get; set; }
        public string period3 { get; set; }
        public string amount1 { get; set; }
        public string amount2 { get; set; }
        public string amount3 { get; set; }
        public string period_type { get; set; }
        public string subscr_date { get; set; }
        public string subscr_effective { get; set; }
        public string subscr_id { get; set; }

        public double Total
        {
            get
            {
                double amount = 0;
                if (double.TryParse(mc_gross, out amount))
                {
                    return amount;
                }
                else
                {
                    return 0;
                }
            }
        }

        public double Fee
        {
            get
            {
                double amount = 0;
                if (double.TryParse(mc_fee, out amount))
                {
                    return amount;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion
    }
}
