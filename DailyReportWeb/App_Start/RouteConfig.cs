using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using System.Linq;
using System.Net.Http.Formatting;

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
                name: "uninstalled-callback",
                url: "uninstalled",
                defaults: new
                {
                    controller = "Home",
                    action = "UninstalledCallback"
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
               name: "SPA",
                url: "home/*",
                defaults: new
                {
                    controller = "Home",
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
				name: "Account",
				routeTemplate: "api/account/{action}",
				defaults: new { controller = "Account" }
			);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            jsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            return config;
        }
    }
}
