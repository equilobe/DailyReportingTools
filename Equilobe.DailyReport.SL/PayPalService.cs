using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Data;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.PayPal;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Views;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class PayPalService : IPayPalService
    {
        public IDataService DataService { get; set; }
        public IRegistrationService RegistrationService { get; set; }

        public void GetStatus(byte[] parameters, PayPalCheckoutInfo payPalCheckoutInfo, UserManager<ApplicationUser> userManager)
        {
            try
            {
                SaveLog(payPalCheckoutInfo);
            }
            catch
            {

            }

            string status = Verify(true, parameters);

            if (status == "INVALID")
                return;

            var subscriptionContext = new SubscriptionContext();
            subscriptionContext.TxnType = payPalCheckoutInfo.txn_type;
            subscriptionContext.TxnId = payPalCheckoutInfo.txn_id;


            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionSignup)
            {
                var subscriptionDate = new DateTime();
                var trialEndDate = new DateTime();

                using(var db = new ReportsDb())
                {
                    var sub = db.SubscriptionDetails.SingleOrDefault(s => s.SubscriptionId == payPalCheckoutInfo.subscr_id && s.TxnType == PayPalVariables.SubscriptionSignup);

                    if (sub != null)
                        return;
                }


                if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
                {
                    //trial period
                    string subscriptionDateTZ = payPalCheckoutInfo.subscr_date.Replace("PDT", "-0700");
                    subscriptionDate = Convert.ToDateTime(subscriptionDateTZ);
                    trialEndDate = GetTrialEndDate(subscriptionDate, payPalCheckoutInfo.period1);
                }

                if (payPalCheckoutInfo.custom == null)
                    return;

                var registrationInfo = JsonConvert.DeserializeObject<RegisterModel>(payPalCheckoutInfo.custom);

                try
                {
                    RegistrationService.RegisterUser(registrationInfo, userManager);
                }
                catch(Exception)
                {
                    RemoveUser(registrationInfo);

                    return;
                }

                var username = registrationInfo.Email;
                var instanceUrl = registrationInfo.BaseUrl;              

                subscriptionContext.Username = username;
                subscriptionContext.BaseUrl = instanceUrl;
                subscriptionContext.PaymentGross = payPalCheckoutInfo.Total;
                subscriptionContext.SubscriptionId = payPalCheckoutInfo.subscr_id;
                subscriptionContext.SubscriptionDate = subscriptionDate;                

                if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
                {
                    subscriptionContext.TrialStartDate = subscriptionDate;
                    subscriptionContext.TrialEndDate = trialEndDate;
                }

                DataService.ActivateInstance(username, instanceUrl);
                DataService.AddSubscriptionDetails(subscriptionContext);

            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionCanceled)
            {
                DataService.DeactivateInstance(payPalCheckoutInfo.subscr_id);
            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionExpired)
            {
                DataService.DeactivateInstance(payPalCheckoutInfo.subscr_id);
            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionPayment)
            {
                //check that the payment_status is Completed                 
                if (payPalCheckoutInfo.payment_status == "Completed")
                {
                    if (TransactionProcessed(payPalCheckoutInfo.txn_id))
                        return;


                    //trial period is over. payment is received

                    //check that txn_id has not been previously processed to prevent duplicates                      

                    //check that receiver_email is your Primary PayPal email                                          

                    //check that payment_amount/payment_currency are correct                       

                    //process payment/refund/etc               

                }

            }

            if(payPalCheckoutInfo.payment_status == "Refunded" || payPalCheckoutInfo.reason_code == "refund")
            {
                //refund 
            }

        }

        #region Helpers

        private bool TransactionProcessed(string transactionId)
        {
            using(var db = new ReportsDb())
            {
                var transaction = db.SubscriptionDetails.SingleOrDefault(s => s.TxnId == transactionId);

                return transaction != null;
            }
        }

        private static void SaveLog(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + @"TemporaryLogs\ipnLogs.xml";
            var xml = File.ReadAllText(path);
            var logs = Deserialization.XmlDeserialize<List<PayPalLog>>(xml);
            if (logs == null)
                logs = new List<PayPalLog>();

            logs.Add(new PayPalLog
            {
                PayPalInfo = payPalCheckoutInfo,
                Date = DateTime.Now
            });

            var newXml = Serialization.XmlSerialize(logs);
            File.WriteAllText(path, newXml);
        }

        private static void RemoveUser(RegisterModel registrationInfo)
        {
            using (var db = new ReportsDb())
            {
                var user = db.Users.SingleOrDefault(u => u.Email == registrationInfo.Email && !u.EmailConfirmed);
                if (user != null)
                    db.Users.Remove(user);
                db.SaveChanges();
            }
        }

        string Verify(bool isSandbox, byte[] parameters)
        {

            string response = "";

            string url = isSandbox ?
              "https://www.sandbox.paypal.com/cgi-bin/webscr" : "https://www.paypal.com/cgi-bin/webscr";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            //must keep the original intact and pass back to PayPal with a _notify-validate command
            string data = Encoding.ASCII.GetString(parameters);
            data += "&cmd=_notify-validate";

            webRequest.ContentLength = data.Length;

            //Send the request to PayPal and get the response                 
            using (StreamWriter streamOut = new StreamWriter(webRequest.GetRequestStream(), System.Text.Encoding.ASCII))
            {
                streamOut.Write(data);
                streamOut.Close();
            }

            using (StreamReader streamIn = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                response = streamIn.ReadToEnd();
                streamIn.Close();
            }

            return response;
        }

        DateTime GetTrialEndDate(DateTime startDate, string trialPeriod)
        {
            var period = trialPeriod.Split(' ');
            var amount = Int32.Parse(period[0]);
            var intervalType = period[1];

            if (intervalType == "D")
                return startDate.AddDays(amount);

            if (intervalType == "W")
                return startDate.AddDays(amount * 7);

            if (intervalType == "M")
                return startDate.AddMonths(amount);

            return startDate.AddYears(amount);
        }

        #endregion
    }
}
