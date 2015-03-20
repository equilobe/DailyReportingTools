using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.SL;

namespace SourceControlLogReporter
{
    static class DependencyInjection
    {
        public static IContainer Container { get; private set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterServicesFromAssembly<GitHubService>();

            builder.RegisterType<GitHubReport>().AsSelf().PropertiesAutowired();
            builder.RegisterType<SvnReport>().AsSelf().PropertiesAutowired();

            Container = builder.Build();
        }

        public static void RegisterServicesFromAssembly<TAssemblyType>(this ContainerBuilder builder)
        {
            var types = typeof(TAssemblyType).Assembly.GetTypes()
                            .Where(t => t.IsClass && !t.IsAbstract)
                            .Where(t => t.IsAssignableTo<IService>())
                            .ToArray();

            builder.RegisterTypes(types)
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();
        }
    }
}
