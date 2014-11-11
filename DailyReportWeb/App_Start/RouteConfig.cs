using System.Web.Mvc;
using System.Web.Routing;

namespace DailyReportWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "descriptor",
                url: "atlassian-connect.json",
                defaults: new
                {
                    controller = "Home",
                    action = "Descriptor"
                }
            );
            routes.MapRoute(
                name: "installed-callback",
                url: "installed",
                defaults: new
                {
                    controller = "Home",
                    action = "InstalledCallback"
                }
            );
            routes.MapRoute(
                name: "test-plugin",
                url: "test-plugin",
                defaults: new
                {
                    controller = "Home",
                    action = "Plugin"
                }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional
                }
            );
        }
    }
}
