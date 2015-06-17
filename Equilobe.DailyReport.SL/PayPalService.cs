using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Paypal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class PayPalService : IPayPalService
    {
        public void GetStatus(byte[] parameters, PayPalCheckoutInfo payPalCheckoutInfo)
        {

            //verify the transaction             
            string status = Verify(true, parameters);

            if (status == "VERIFIED")
            {

                if (payPalCheckoutInfo.txn_type == "subscr_signup")
                {
                    // register user/instance
                }

                if (payPalCheckoutInfo.txn_type == "subscr_cancel")
                {
                    // disable instance
                }

                //check that the payment_status is Completed                 
                if (payPalCheckoutInfo.payment_status == "Completed")
                {

                    //check that txn_id has not been previously processed to prevent duplicates                      

                    //check that receiver_email is your Primary PayPal email                                          

                    //check that payment_amount/payment_currency are correct                       

                    //process payment/refund/etc      

                    if (payPalCheckoutInfo.custom == null)
                        return;



                    var username = "";
                    var instanceUrl = "";

                    using(var db = new ReportsDb())
                    {
                        var instance = db.InstalledInstances.SingleOrDefault(i => i.User.Email == username && i.BaseUrl == instanceUrl);
                        if (instance == null)
                            return;

                    }

                }
                else
                {
                    //log response/ipn data for manual investigation             
                }

            }
            else if (status == "INVALID")
            {

                //log for manual investigation             
            }

        }

        #region Helpers
        private string Verify(bool isSandbox, byte[] parameters)
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
        #endregion
    }
}
