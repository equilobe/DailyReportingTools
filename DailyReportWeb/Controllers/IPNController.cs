using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.PayPal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Equilobe.DailyReport.Models.Storage;
using Microsoft.AspNet.Identity;

namespace DailyReportWeb.Controllers
{
    public class IPNController : BaseMvcController
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