using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            DependencyInjection.Init();
            var mainFlow = DependencyInjection.Container.Resolve<JiraReportMainFlowProcessor>();
            mainFlow.Execute(args);
        }

    }
}
