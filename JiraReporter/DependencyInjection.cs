using Autofac;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.SL;
using JiraReporter.Services;
using JiraReporter.SourceControl;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    static class DependencyInjection
    {

        public static IContainer Container { get; private set; }

        public static void Init()
        {
            var builder = new ContainerBuilder();

            builder.RegisterServicesFromAssembly<JiraService>();
            builder.RegisterServicesFromAssembly<BitBucketService>();
            builder.RegisterServicesFromAssembly<ReportGeneratorService>();

            builder.RegisterType<JiraReportMainFlowProcessor>().AsSelf().PropertiesAutowired();
            builder.RegisterType<GitHubReportSourceControl>().AsSelf().PropertiesAutowired();
            builder.RegisterType<SvnReportSourceControl>().AsSelf().PropertiesAutowired();
            builder.RegisterType<BitBucketSourceControl>().AsSelf().PropertiesAutowired();

            Container = builder.Build();
        }

        public static void RegisterServicesFromAssembly<TAssemblyType>(this ContainerBuilder builder)
        {               
            var types = typeof(TAssemblyType).Assembly.GetTypes()
                            .Where(t=> t.IsClass && !t.IsAbstract)
                            .Where(t => t.IsAssignableTo<IService>())
                            .ToArray();

            builder.RegisterTypes(types)
                .AsImplementedInterfaces()
                .PropertiesAutowired()
                .SingleInstance();
        }
    }
}
