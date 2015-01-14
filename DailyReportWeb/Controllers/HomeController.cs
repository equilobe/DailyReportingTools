using Atlassian.Connect;
using Atlassian.Connect.Entities;
using Atlassian.Connect.Jwt;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Web;
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

        [JwtAuthentication]
        public ActionResult Plugin()
        {
            var client = Request.CreateConnectHttpClient("com.equilobe.drt");

            var response = client.GetAsync("rest/api/latest/project").Result;
            var results = response.Content.ReadAsStringAsync().Result;

            dynamic model = new ExpandoObject();
            model.projects = results;
            return View(model);
        }

        [HttpGet]
        public ActionResult Descriptor()
        {
            var descriptor = new ConnectDescriptor()
            {
                name = "Daily Report Tool",
                description = "A Connect add-on that makes JIRA info available to Daily Report Tool",
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
                apiVersion = 1,
                modules = new
                {
                    generalPages = new[] 
                    { 
                        new 
                        {
                            name = new 
                            {
                                value = "test DRT"
                            },
                            url = "/test-plugin",
                            key = "drt-test",
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
            descriptor.scopes.Add("READ");

            return Json(descriptor, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InstalledCallback()
        {
            SecretKeyPersister.SaveSecretKey(Request);

            return Content(String.Empty);
        }

        static class SecretKeyPersister
        {
            public static void SaveSecretKey(HttpRequestBase request)
            {
                using (var dbContext = new InstancesContext())
                {
                    var bodyText = new System.IO.StreamReader(request.InputStream).ReadToEnd();

                    var instanceData = JsonConvert.DeserializeObject<InstalledInstance>(bodyText);

                    var baseUrl = instanceData.BaseUrl;
                    var sharedSecret = instanceData.SharedSecret;

                    dbContext.InstalledInstances.Add(instanceData);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}