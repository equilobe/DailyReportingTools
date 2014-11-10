using Atlassian.Connect;
using System;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult Plugin()
        {
            var descriptor = new ConnectDescriptor()
            {
                name = "Daily Reporting Tool",
                description = "A Connect add-on that makes JIRA info available to Daily Reporting Tool",
                key = "com.equilobe.drt",
                vendor = new ConnectDescriptorVendor()
                {
                    name = "Equilobe Software",
                    url = "http://equilobe.com/"
                },
                authentication = new
                {
                    type = "jwt"
                },
                lifecycle = new
                {
                    installed = "/installed"
                },
                modules = new
                {
                    //generalPages = new[] 
                    //{ 
                    //    new 
                    //    {
                    //        name = new 
                    //        {
                    //            value = "Daily Reporting Tool"
                    //        },
                    //        url = "/Home/Index",
                    //        key = "drt-home",
                    //        location = "system.top.navigation.bar",
                    //        conditions = new[]
                    //        {
                    //            new
                    //            {
                    //                condition = "user_is_logged_in"
                    //            }
                    //        }
                    //    }
                    //}
                }
            };

            descriptor.SetBaseUrlFromRequest(Request);
            descriptor.scopes.Add("READ");

            return Json(descriptor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InstalledCallback()
        {
            //SecretKeyPersister.SaveSecretKey(Request);

            return Content(String.Empty);
        }
    }
}