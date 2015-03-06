using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using Newtonsoft.Json;
using System;
using System.Configuration;
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
        public ActionResult Descriptor()
        {
            var descriptor = new
            {
                name = "Daily Report Tool",
                description = "A Connect add-on that makes JIRA info available to Daily Report Tool",
                key = ConfigurationManager.AppSettings["addonKey"],
                baseUrl = UriExtensions.GetHostUrl(Request.Url.OriginalString),
                vendor = new
                {
                    name = "Equilobe Software",
                    url = "http://equilobe.com/"
                },
                links = new
                {
                    self = UriExtensions.GetHostUrl(Request.Url.OriginalString) + "/howto",
                    documentation = UriExtensions.GetHostUrl(Request.Url.OriginalString) + "/docs"
                },
                apiVersion = 1,
                authentication = new
                {
                    type = "jwt"
                },
                lifecycle = new
                {
                    installed = "/installed",
                    uninstalled = "/uninstalled"
                },
                scopes = new[]
                {
                    "PROJECT_ADMIN"
                },
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
                                    condition = "user_is_admin"
                                }
                            }
                        }
                    }
                }
            };

            return Json(descriptor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InstalledCallback()
        {
            new DataService().Save(Request);

            return Content(String.Empty);
        }

        [HttpPost]
        public ActionResult UninstalledCallback()
        {
            var bodyText = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var baseUrl = JsonConvert.DeserializeObject<InstalledInstance>(bodyText).BaseUrl;

            new TaskSchedulerService().Delete(new DataService().GetUniqueProjectsKey(baseUrl));
            new DataService().Delete(baseUrl);

            return Content(String.Empty);
        }
    }
}