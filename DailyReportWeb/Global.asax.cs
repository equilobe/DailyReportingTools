using System;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;

namespace DailyReportWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            DependencyInjection.Init();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configuration.InitApiRoutes();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteTable.Routes.RegisterRoutes();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();

            Log.Error(ex, "Something went wrong");
        }
    }
}
