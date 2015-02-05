using Atlassian.Connect;
using Atlassian.Connect.Entities;
using Atlassian.Connect.Jwt;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Storage;
using System.Configuration;

namespace DailyReportWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            using (var db = new ReportsDb())
            {
                db.ReportSettings.Add(new ReportSettings { BaseUrl = "nice" });
                db.SaveChanges();
            }
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
        public ActionResult Descriptor()
        {
            var descriptor = new ConnectDescriptor()
            {
                name = "Daily Report Tool",
                description = "A Connect add-on that makes JIRA info available to Daily Report Tool",
                key = ConfigurationManager.AppSettings["addonKey"],
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
                apiVersion = 1,
                modules = new
                {
                    generalPages = new[] 
                    { 
                        new 
                        {
                            name = new 
                            {
                                value = "DRT"
                            },
                            url = "/setup",
                            key = "drt-setup",
                            location = "system.top.navigation.bar",
                            conditions = new[]
                            {
                                new
                                {
                                    condition = "user_is_logged_in"
                                }
                            }
                        }
                    }
                }
            };

            descriptor.SetBaseUrlFromRequest(Request);
            descriptor.scopes.Add("PROJECT_ADMIN");

            return Json(descriptor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InstalledCallback()
        {
            SecretKeyPersister.SaveSecretKey(Request);

            return Content(String.Empty);
        }
    }
}