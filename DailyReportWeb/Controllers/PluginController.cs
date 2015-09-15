using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class PluginController : Controller
    {
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public IDataService DataService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }

        [HttpGet]
        public ActionResult Index()
        {
            var descriptor = new
            {
                name = "Daily Report Tool",
                description = "A Connect add-on that makes JIRA info available to Daily Report Tool",
                key = ConfigurationService.GetAddonKey(),
                baseUrl = UrlExtensions.GetHostUrl(Request.Url.OriginalString),
                vendor = new
                {
                    name = "Equilobe Software",
                    url = "http://equilobe.com/"
                },
                links = new
                {
                    self = UrlExtensions.GetHostUrl(Request.Url.OriginalString) + "/app/welcome",
                    documentation = UrlExtensions.GetHostUrl(Request.Url.OriginalString) + "/app/howItWorks"
                },
                apiVersion = 1,
                authentication = new
                {
                    type = "jwt"
                },
                lifecycle = new
                {
                    installed = "/plugin/installed",
                    uninstalled = "/plugin/uninstalled"
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
                                value = "DailyReport"
                            },
                            url = "/",
                            key = "app",
                            location = "system.top.navigation.bar"//,
                            //conditions = new[]
                            //{
                            //    new
                            //    {
                            //        condition = "user_is_admin"
                            //    }
                            //}
                        }
                    }
                }
            };

            return Json(descriptor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Installed()
        {
            var bodyText = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
          //  var instanceData = JsonConvert.DeserializeObject<InstalledInstance>(bodyText);
          //  DataService.SaveInstance(instanceData);

            return Content(String.Empty);
        }

        [HttpPost]
        public ActionResult Uninstalled()
        {
            var bodyText = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
            var pluginKey = JsonConvert.DeserializeObject<InstalledInstance>(bodyText).ClientKey;

           // var projectKeys = DataService.GetUniqueProjectsKey(pluginKey);
          //  TaskSchedulerService.DeleteMultipleTasks(new ProjectListContext
         //   {
         //       UniqueProjectKeys = projectKeys
         //   });

          //  DataService.DeleteInstance(pluginKey);

            return Content(String.Empty);
        }
    }
}