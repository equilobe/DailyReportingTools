using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Data;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.PayPal;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Views;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL.Interfaces;
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
using System.Transactions;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class PayPalService : IPayPalService
    {
        public IDataService DataService { get; set; }
        public IRegistrationService RegistrationService { get; set; }
        public IEmailService EmailService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }
        public IOwinService OwinService { get; set; }

        public void GetStatus(byte[] parameters, PayPalCheckoutInfo payPalCheckoutInfo)
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

            TryProcessIPN(id, payPalCheckoutInfo);

            ProcessIPNLogs(id);

            //Task.Factory.StartNew(() => ProcessIPN(payPalCheckoutInfo, id));
           // Task.Factory.StartNew(() => ProcessIPNLogs(id));
        }

        public bool ProcessIPN(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionSignup)
                return HandleSubscriptionSignUp(payPalCheckoutInfo, ipnId);

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionCanceled)
            {
                HandleSubscriptionCanceling(payPalCheckoutInfo, ipnId);
                return true;
            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionExpired)
            {
                HandleSubscriptionExpiration(payPalCheckoutInfo, ipnId);
                return true;
            }

            if (payPalCheckoutInfo.txn_type == PayPalVariables.SubscriptionPayment)
            {
                return HandleSubscriptionPayment(payPalCheckoutInfo, ipnId);
            }

            if (payPalCheckoutInfo.payment_status == "Refunded" || payPalCheckoutInfo.reason_code == "refund")
            {
                return HandleRefund(payPalCheckoutInfo, ipnId);
            }

            SetLogProcessed(ipnId);
            return true;
        }

        public void ProcessIPNLogs(long skipLogId)
        {
            var logs = new List<IPNLog>();

            using (var db = new ReportsDb())
            {
                logs = db.IPNLogs.Where(l => l.DateProcessed == null && l.Id != skipLogId).ToList();
            }

            foreach (var log in logs)
            {
                var paypalInfo = Deserialization.XmlDeserialize<PayPalCheckoutInfo>(log.SerializedPayPalInfo);
                TryProcessIPN(log.Id, paypalInfo);
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

        private void TryProcessIPN(long id, PayPalCheckoutInfo paypalInfo)
        {
            try
            {
                var result = ProcessIPN(paypalInfo, id);
            }
            catch (Exception ex)
            {
                SaveException(id, ex);
            }
        }

        void HandleSubscriptionCanceling(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            var instance = DataService.GetInstance(payPalCheckoutInfo.subscr_id);
            var user = DataService.GetUser(instance.UserId);

            var emailModel = GetSubscriptionCancelingEmailModel(payPalCheckoutInfo, instance);

            var subject = "Daily Report | Subscription Cancelled";

            var body = GetSubscriptionCancelingEmailBody(emailModel);

            var message = EmailService.GetHtmlMessage(user.Email, subject, body);

            EmailService.SendEmail(message);

            SetLogProcessed(ipnId);
        }

        EmailNotification GetSubscriptionCancelingEmailModel(PayPalCheckoutInfo payPalCheckoutInfo, InstalledInstance instance)
        {
            var emailNotification = new EmailNotification
            {
                InstanceUrl = instance.BaseUrl,
                ExpirationDate = instance.ExpirationDate,
                ExpirationDateString = instance.ExpirationDate.ToLongDateString(),
                InstanceId = instance.Id,
                DailyReportAppUrl = ConfigurationService.GetWebBaseUrl()
            };
            emailNotification.SubscribeInstanceUrl = emailNotification.DailyReportAppUrl + "/app/" + emailNotification.InstanceId + "/projects";

            return emailNotification;
        }

        string GetSubscriptionCancelingEmailBody(EmailNotification model)
        {
            var template = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"Views\Email\SubscriptionCanceling.cshtml");

            return RazorEngine.Razor.Parse(template, model);
        }

        bool HandleRefund(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            var paymentContext = GetPaymentContext(payPalCheckoutInfo);
            try
            {
                DataService.SavePayment(paymentContext);
            }
            catch
            {
                return false;
            }

            if (!CheckSubscriptionPaymentSituation(payPalCheckoutInfo.subscr_id))
                DataService.DeactivateInstance(payPalCheckoutInfo.subscr_id);

            SetLogProcessed(ipnId);
            return true;
        }

        bool HandleSubscriptionPayment(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            if (payPalCheckoutInfo.payment_status == PayPalVariables.PaymentCompleted)
            {
                if (TransactionProcessed(payPalCheckoutInfo.txn_id))
                {
                    SetLogProcessed(ipnId);
                    return true;
                }

                var paymentContext = GetPaymentContext(payPalCheckoutInfo);

                try
                {
                    DataService.SavePayment(paymentContext);
                }
                catch
                {
                    return false;
                }

                var subscription = DataService.GetSubscription(paymentContext.SubscriptionId);
                if (CheckPayment(payPalCheckoutInfo))
                    DataService.SetInstanceExpirationDate(payPalCheckoutInfo.subscr_id, GetPeriodEndDate(DateTime.Now, subscription.SubscriptionPeriod));
            }

            SetLogProcessed(ipnId);
            return true;
        }

        void HandleSubscriptionExpiration(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            DataService.DeactivateInstance(payPalCheckoutInfo.subscr_id);
            SetLogProcessed(ipnId);
        }

        bool HandleSubscriptionSignUp(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            var subscriptionContext = new SubscriptionContext();
            string subscriptionDateTZ = payPalCheckoutInfo.subscr_date.Replace("PDT", "-0700");
            subscriptionContext.SubscriptionDate = Convert.ToDateTime(subscriptionDateTZ);

            if (WasSubscriptionAlreadyProcessed(payPalCheckoutInfo, ipnId))
                return true;

            if (payPalCheckoutInfo.custom == null)
            {
                SetLogProcessed(ipnId);
                return true;
            }

            var registrationInfo = JsonConvert.DeserializeObject<RegisterModel>(payPalCheckoutInfo.custom);

            var register = RegisterInstance(ipnId, registrationInfo, payPalCheckoutInfo);
            if (!register)
                return true;

            SetSubscriptionContext(payPalCheckoutInfo, subscriptionContext, registrationInfo);

            try
            {
                DataService.SaveSubscription(subscriptionContext);
                if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
                    DataService.SetInstanceExpirationDate(subscriptionContext.SubscriptionId, subscriptionContext.TrialEndDate.Value);
            }
            catch
            {
                return false;
            }

            SetLogProcessed(ipnId);
            return true;
        }

        void SetSubscriptionContext(PayPalCheckoutInfo payPalCheckoutInfo, SubscriptionContext subscriptionContext, RegisterModel registrationInfo)
        {

            if (!string.IsNullOrEmpty(registrationInfo.Email))
                subscriptionContext.Username = registrationInfo.Email;
            if (registrationInfo.InstanceId != null)
                subscriptionContext.InstanceId = registrationInfo.InstanceId;

            subscriptionContext.BaseUrl = registrationInfo.BaseUrl;
            subscriptionContext.SubscriptionId = payPalCheckoutInfo.subscr_id;
            subscriptionContext.SubscriptionPeriod = payPalCheckoutInfo.period3;

            if (!string.IsNullOrEmpty(payPalCheckoutInfo.period1))
            {
                subscriptionContext.TrialEndDate = GetPeriodEndDate(subscriptionContext.SubscriptionDate, payPalCheckoutInfo.period1);
            }
        }

        bool WasSubscriptionAlreadyProcessed(PayPalCheckoutInfo payPalCheckoutInfo, long ipnId)
        {
            using (var db = new ReportsDb())
            {
                var sub = db.Subscriptions.SingleOrDefault(s => s.Id == payPalCheckoutInfo.subscr_id);

                if (sub != null)
                {
                    SetLogProcessed(ipnId);
                    return true;
                }
            }

            return false;
        }

        bool RegisterInstance(long ipnId, RegisterModel registrationInfo, PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var instance = new InstalledInstance();

            if (registrationInfo.InstanceId != null)
            {
                instance = DataService.GetInstance(registrationInfo.InstanceId.Value);
                DataService.SetInstanceExpirationDate(registrationInfo.InstanceId.Value, GetPeriodEndDate(DateTime.Now, payPalCheckoutInfo.period3));
            }
            else
                return SaveInstance(ipnId, registrationInfo);

            return true;
        }

        bool SaveInstance(long ipnId, RegisterModel registrationInfo)
        {
            var saveInstance = DataService.SaveInstance(registrationInfo);

            if (saveInstance.HasError)
            {
                SetLogProcessed(ipnId);
                return false;
            }

            return true;
        }

        void SetLogProcessed(long id)
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

        bool IsOnTrial(Subscription subscription)
        {
            return DateTime.Now < subscription.TrialEndDate;
        }

        bool CheckPayment(PayPalCheckoutInfo paypalInfo)
        {
            if (paypalInfo.Total < 10)
                return false;

            return true;
        }

        PaymentContext GetPaymentContext(PayPalCheckoutInfo checkoutInfo)
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

        bool TransactionProcessed(string transactionId)
        {
            var transaction = new Payment();
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
            {
                using (var db = new ReportsDb())
                {
                    if (db.Payments == null)
                        return false;

                    transaction = db.Payments.SingleOrDefault(s => s.TransactionId == transactionId);

                }
                scope.Complete();
                return transaction != null;
            }

        }

        long SaveLog(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var log = GetLog(payPalCheckoutInfo);

            using (var db = new ReportsDb())
            {
                db.IPNLogs.Add(log);
                db.SaveChanges();
                return log.Id;
            }
        }

        static IPNLog GetLog(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var serializedInfo = Serialization.XmlSerialize(payPalCheckoutInfo);

            var log = new IPNLog
            {
                SerializedPayPalInfo = serializedInfo,
                DateAdded = DateTime.Now
            };

            return log;
        }

        static void RemoveUser(RegisterModel registrationInfo)
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
            string url = isSandbox ?
              "https://www.sandbox.paypal.com/cgi-bin/webscr" : "https://www.paypal.com/cgi-bin/webscr";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";

            string data = Encoding.ASCII.GetString(parameters);
            data += "&cmd=_notify-validate";

            webRequest.ContentLength = data.Length;

            string response = GetResponse(webRequest, data);

            return response;
        }

        static string GetResponse(HttpWebRequest webRequest, string data)
        {
            string response = "";

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

        private void SaveException(long logId, Exception ex)
        {
            var error = ex.Message + "\r\n" + ex.StackTrace + "\r\n";
            if (ex.InnerException != null)
                error += "InnerException: \r\n" + ex.InnerException.Message;

            DataService.SaveIpnLogError(logId, error);
        }

        #endregion
    }
}
