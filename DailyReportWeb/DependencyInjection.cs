using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.Owin;
using Autofac.Integration.WebApi;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

namespace DailyReportWeb
{
    static class DependencyInjection
    {

        public static IContainer Container { get; private set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterServicesFromAssembly<JiraService>();

            builder.RegisterControllers(typeof(MvcApplication).Assembly)
                   .PropertiesAutowired();

            builder.RegisterApiControllers(typeof(MvcApplication).Assembly)
                   .PropertiesAutowired();

            builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);

            Container = builder.Build();

            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(Container);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(Container));
        }

        public static void RegisterServicesFromAssembly<TAssemblyType>(this ContainerBuilder builder)
        {               
            var types = typeof(TAssemblyType).Assembly.GetTypes()
                            .Where(t=> t.IsClass && !t.IsAbstract)
                            .Where(t => t.IsAssignableTo<IService>())
                            .ToArray();

            builder.RegisterTypes(types)
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }
    }
}
