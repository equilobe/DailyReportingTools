using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Configuration;
using RazorEngine;
using System.Net.Mail;
using System.Net;
using RazorEngine.Templating;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Octokit;
using Octokit.Internal;
using System.Globalization;
using SourceControlLogReporter;
using Equilobe.DailyReport.Models.ReportPolicy;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.SourceControl;
using CommandLine;

namespace SourceControlLogReporter
{

    class Program
    {


        private static readonly Dictionary<SourceControlType, Func<Policy, Options, ReportBase>> Processors = new Dictionary<SourceControlType, Func<Policy, Options, ReportBase>>()
        {
            {SourceControlType.GitHub, ReportBase.Create<GitHubReport>},
            {SourceControlType.SVN, ReportBase.Create<SvnReport>}
        };

        static void Main(string[] args)
        {
            try
            {
                SetGlobalSettings();
                ExecuteReporter(args);
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SetGlobalSettings()
        {
            Razor.SetTemplateBase(typeof(RazorEngine.ExtendedTemplateBase<>));
        }

        private static void ExecuteReporter(string[] args)
        {
            Options options = new Options();
            new CommandLineParser().ParseArguments(args, options);
            Policy policy = PolicyService.LoadFromFile(options.PolicyPath);
            var policyService = new PolicyService(policy);
            policyService.SetPolicy();
            options.LoadDates();

            var processor = Processors[policy.SourceControlOptions.Type](policy, options);
            var report = processor.GenerateReport();
            Reporter.WriteReport(policy, report, processor.PathToLog);

            var emailer = new ReportEmailer(policy, options);
            emailer.TrySendEmails();
        }

        private static void SetAppConfigFile()
        {
            var currentConfigPath = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
            var newConfigPath = Path.Combine(Environment.CurrentDirectory, Path.GetFileName(currentConfigPath));
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", newConfigPath);
        }

        
    }
}
