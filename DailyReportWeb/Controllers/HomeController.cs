using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class HomeController : Controller
    {
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public IDataService DataService { get; set; }
        public IPolicyEditorService PolicyService { get; set; }

        public ActionResult Index()
        {
            return View(new RegexValidation());
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
                baseUrl = UrlExtensions.GetHostUrl(Request.Url.OriginalString),
                vendor = new
                {
                    name = "Equilobe Software",
                    url = "http://equilobe.com/"
                },
                links = new
                {
                    self = UrlExtensions.GetHostUrl(Request.Url.OriginalString) + "/howto",
                    documentation = UrlExtensions.GetHostUrl(Request.Url.OriginalString) + "/docs"
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
            var bodyText = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var instanceData = JsonConvert.DeserializeObject<InstalledInstance>(bodyText);
            DataService.SaveInstance(instanceData);

            return Content(String.Empty);
        }

        [HttpPost]
        public ActionResult UninstalledCallback()
        {
            var bodyText = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var baseUrl = JsonConvert.DeserializeObject<InstalledInstance>(bodyText).BaseUrl;

            var projectKeys = DataService.GetUniqueProjectsKey(baseUrl);

            TaskSchedulerService.DeleteMultipleTasks( new ProjectListContext
            {
                UniqueProjectKeys = projectKeys
            });
            DataService.DeleteInstance(baseUrl);

            return Content(String.Empty);
        }
    }
}