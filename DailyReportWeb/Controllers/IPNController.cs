using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Paypal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class IPNController : Controller
    {
        public IPayPalService PayPalService { get; set; }

        public EmptyResult PayPalPaymentNotification(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            byte[] parameters = Request.BinaryRead(Request.ContentLength);

            if (parameters != null)
            {
                PayPalService.GetStatus(parameters, payPalCheckoutInfo);
            }

            return new EmptyResult();
        }
    }
}