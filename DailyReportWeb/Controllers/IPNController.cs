using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Paypal;
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
    public class IPNController : Controller
    {
        public IPayPalService PayPalService { get; set; }

        public EmptyResult PayPalPaymentNotification(PayPalCheckoutInfo payPalCheckoutInfo)
        {
            var userManager = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            byte[] parameters = Request.BinaryRead(Request.ContentLength);

            if (parameters != null)
            {
                PayPalService.GetStatus(parameters, payPalCheckoutInfo, userManager);
            }

            return new EmptyResult();
        }
    }
}