using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Paypal;
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



            //verify the transaction             
            string status = Verify(true, parameters);

            if (status == "INVALID")
                return;


            if (payPalCheckoutInfo.txn_type == "subscr_signup")
            {
                // register user/instance

                var subscriptionDate = new DateTime();
                var trialEndDate = new DateTime();

                using(var db = new ReportsDb())
                {
                    var sub = db.SubscriptionDetails.SingleOrDefault(s => s.SubscriptionId == payPalCheckoutInfo.subscr_id);
                    if (sub != null)
                        return;
                    //maybe update subscription. depends on the case. investigate first
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

                var subscriptionContext = new SubscriptionContext();
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

                DataService.ActivateInstance(subscriptionContext);

            }

            if (payPalCheckoutInfo.txn_type == "subscr_cancel")
            {
                // disable instance
            }

            //check that the payment_status is Completed                 
            if (payPalCheckoutInfo.payment_status == "Completed")
            {

                //trial period is over. payment is received

                //check that txn_id has not been previously processed to prevent duplicates                      

                //check that receiver_email is your Primary PayPal email                                          

                //check that payment_amount/payment_currency are correct                       

                //process payment/refund/etc               

            }
            else
            {
                //log response/ipn data for manual investigation             
            }
        }

        #region Helpers

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
