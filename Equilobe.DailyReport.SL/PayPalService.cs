using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
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
            long id;
            try
            {
                id = SaveLog(payPalCheckoutInfo);
            }
            catch
            {
                return;
            }

            string status = Verify(true, parameters);

            if (status == PayPalVariables.InvalidStatus)
                return;

            ProcessIPN(payPalCheckoutInfo, userManager, id);
            ProcessIPNLogs(userManager, id);
        }

        public void ProcessIPN(PayPalCheckoutInfo payPalCheckoutInfo, UserManager<ApplicationUser> userManager, long ipnId)
        {
            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionSignup)
            {
                var subscriptionContext = new SubscriptionContext();
                string subscriptionDateTZ = payPalCheckoutInfo.subscr_date.Replace("PDT", "-0700");
                subscriptionContext.SubscriptionDate = Convert.ToDateTime(subscriptionDateTZ);

                using (var db = new ReportsDb())
                {
                    var sub = db.Subscriptions.SingleOrDefault(s => s.Id == payPalCheckoutInfo.subscr_id);

                    if (sub != null)
                    {
                        SetLogProcessed(ipnId);
                        return;
                    }
                }

                if (payPalCheckoutInfo.custom == null)
                {
                    SetLogProcessed(ipnId);
                    return;
                }

                var registrationInfo = JsonConvert.DeserializeObject<RegisterModel>(payPalCheckoutInfo.custom);
                var registerUser = RegisterUser(userManager, ipnId, registrationInfo);
                if (!registerUser)
                    return;

                subscriptionContext.Username = registrationInfo.Email;
                subscriptionContext.BaseUrl = registrationInfo.BaseUrl;
                subscriptionContext.SubscriptionId = payPalCheckoutInfo.subscr_id;
                subscriptionContext.SubscriptionPeriod = payPalCheckoutInfo.period3;

                if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
                {
                    subscriptionContext.TrialEndDate = GetPeriodEndDate(subscriptionContext.SubscriptionDate, payPalCheckoutInfo.period1);
                }

                try
                {
                    DataService.SaveSubscription(subscriptionContext);
                    if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
                        DataService.SetInstanceExpirationDate(subscriptionContext.SubscriptionId, subscriptionContext.TrialEndDate.Value);
                }
                catch
                {
                    // DataService.SetInstanceExpirationDate(subscriptionContext.SubscriptionId, DateTime.Now.AddMinutes(-1));
                    //  SetLogProcessed(ipnId);
                    return;
                }

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
                if (payPalCheckoutInfo.payment_status == PayPalVariables.PaymentCompleted)
                {
                    if (TransactionProcessed(payPalCheckoutInfo.txn_id))
                    {
                        SetLogProcessed(ipnId);
                        return;
                    }

                    var paymentContext = GetPaymentContext(payPalCheckoutInfo);

                    try
                    {
                        DataService.SavePayment(paymentContext);
                    }
                    catch
                    {
                        return;
                    }

                    var subscription = DataService.GetSubscription(paymentContext.SubscriptionId);
                    if (CheckPayment(payPalCheckoutInfo))
                        DataService.SetInstanceExpirationDate(payPalCheckoutInfo.subscr_id, GetPeriodEndDate(DateTime.Now, subscription.SubscriptionPeriod));


                    //if (CheckPayment(payPalCheckoutInfo) && !DataService.IsInstanceActive(payPalCheckoutInfo.subscr_id))
                    //    DataService.SetInstanceExpirationDate(payPalCheckoutInfo.subscr_id, DateTime.Now.AddMonths(1));
                    // will not be used anymore. if instance subscription expires, new one should be made

                    //trial period is over. payment is received

                    //check that txn_id has not been previously processed to prevent duplicates                      

                    //check that receiver_email is your Primary PayPal email                                          

                    //check that payment_amount/payment_currency are correct                       

                    //process payment/refund/etc               

                }

            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionFailed)
            {

            }

            if (payPalCheckoutInfo.payment_status == "Refunded" || payPalCheckoutInfo.reason_code == "refund")
            {
                var paymentContext = GetPaymentContext(payPalCheckoutInfo);
                try
                {
                    DataService.SavePayment(paymentContext);
                }
                catch
                {
                    return;
                }

                if (!CheckSubscriptionPaymentSituation(payPalCheckoutInfo.subscr_id))
                    DataService.DeactivateInstance(payPalCheckoutInfo.subscr_id);
            }

            SetLogProcessed(ipnId);
        }

        private bool RegisterUser(UserManager<ApplicationUser> userManager, long ipnId, RegisterModel registrationInfo)
        {
            if (string.IsNullOrEmpty(registrationInfo.Email))
                return true;

            var registrationResult = new SimpleResult();
            try
            {
                registrationResult = RegistrationService.RegisterUser(registrationInfo, userManager);
                if (registrationResult.Message == ApplicationErrors.ValidationError || registrationResult.Message == ApplicationErrors.UserAlreadyCreated)
                {
                    SetLogProcessed(ipnId);
                    return false;
                }
            }
            catch (Exception)
            {
                //RemoveUser(registrationInfo);
                //SetLogProcessed(ipnId);
                return false;
            }

            return true;
        }


        public void ProcessIPNLogs(UserManager<ApplicationUser> userManager, long skipLogId)
        {
            var logs = new List<IPNLog>();

            using (var db = new ReportsDb())
            {
                logs = db.IPNLogs.Where(l => l.DateProcessed == null && l.Id != skipLogId).ToList();
            }

            foreach (var log in logs)
            {
                var paypalInfo = Deserialization.XmlDeserialize<PayPalCheckoutInfo>(log.SerializedPayPalInfo);
                ProcessIPN(paypalInfo, userManager, log.Id);
            }
        }

        public bool CheckSubscriptionPaymentSituation(string subscriptionId)
        {
            using (var db = new ReportsDb())
            {
                var subscription = db.Subscriptions.SingleOrDefault(s => s.Id == subscriptionId);
                if (subscription == null)
                    return false;

                if (SubscriptionPaidPresentMonth(subscription))
                    return true;

                return false;
            }
        }

        #region Helpers

        private static void SetLogProcessed(long id)
        {
            using (var db = new ReportsDb())
            {
                var log = db.IPNLogs.Single(l => l.Id == id);
                log.DateProcessed = DateTime.Now;
                db.SaveChanges();
            }
        }

        bool SubscriptionPaidPresentMonth(Subscription subscription)
        {
            var monthPayments = subscription.Payments.Where(p => CheckPaymentDateMonth(p.PaymentDate, subscription.SubscriptionDate) && p.Gross >= 10).ToList();

            var refunds = subscription.Payments.Where(p => CheckPaymentDateMonth(p.PaymentDate, subscription.SubscriptionDate) && p.Gross < 0).ToList();

            var amountPaid = monthPayments.Sum(p => p.Gross) + refunds.Sum(p => p.Gross);

            return amountPaid >= 10;
        }

        bool CheckPaymentDateMonth(DateTime paymentDate, DateTime subscriptionDate)
        {
            if (paymentDate >= subscriptionDate && paymentDate < subscriptionDate.AddMonths(1))
                return true;

            return false;
        }

        static bool IsOnTrial(Subscription subscription)
        {
            return DateTime.Now < subscription.TrialEndDate;
        }

        bool CheckPayment(PayPalCheckoutInfo paypalInfo)
        {
            if (paypalInfo.Total < 10)
                return false;

            return true;
        }

        private PaymentContext GetPaymentContext(PayPalCheckoutInfo checkoutInfo)
        {
            return new PaymentContext
            {
                Fee = checkoutInfo.Fee,
                Gross = checkoutInfo.Total,
                SubscriptionId = checkoutInfo.subscr_id,
                Currency = checkoutInfo.mc_currency,
                TransactionId = checkoutInfo.txn_id,
                PaymentDate = checkoutInfo.TrxnDate,
                Status = checkoutInfo.payment_status,
                ParentTransactionId = checkoutInfo.parent_txn_id,
                Type = checkoutInfo.payment_type
            };
        }

        private bool TransactionProcessed(string transactionId)
        {
            using (var db = new ReportsDb())
            {
                if (db.Payments == null)
                    return false;

                var transaction = db.Payments.SingleOrDefault(s => s.TransactionId == transactionId);

                return transaction != null;
            }
        }

        private static long SaveLog(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var log = GetLog(payPalCheckoutInfo);

            using (var db = new ReportsDb())
            {
                db.IPNLogs.Add(log);
                db.SaveChanges();
                return log.Id;
            }
        }


        private static IPNLog GetLog(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var serializedInfo = Serialization.XmlSerialize(payPalCheckoutInfo);

            var log = new IPNLog
            {
                SerializedPayPalInfo = serializedInfo,
                DateAdded = DateTime.Now
            };

            return log;
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

        DateTime GetPeriodEndDate(DateTime startDate, string trialPeriod)
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
