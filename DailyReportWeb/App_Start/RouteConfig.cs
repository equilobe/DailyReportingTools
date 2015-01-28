using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
namespace DailyReportWeb
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(this RouteCollection routes)
        {
            
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "descriptor",
                url: "atlassian-connect",
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
				name: "setup-policies",
				url: "setup",
                defaults: new
                {
					controller = "Policy",
					action = "Index"
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

        public static HttpConfiguration InitApiRoutes(this HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            return config;
        }
    }
}
