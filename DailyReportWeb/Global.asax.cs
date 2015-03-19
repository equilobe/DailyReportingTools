using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

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
        }

		//protected void Application_AuthorizeRequest(object sender, System.EventArgs e)
		//{
		//	AuthenticationHelpers.SetUser();
		//}
    }
}
