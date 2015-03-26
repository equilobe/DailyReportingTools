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
                name: "SPA-entrypoint",
                url: "",
                defaults: new
                {
                    controller = "Home",
                    action = "Index"
                }
            );

            routes.MapRoute(
               name: "SPA",
                url: "app/{*anyOtherUrlSegment}",
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
