using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SvnLogReporter.Model;
using System.Configuration;
using RazorEngine;
using System.Net.Mail;
using System.Net;
using RazorEngine.Templating;
using System.Diagnostics;
using System.Net.NetworkInformation;
using CommandLine;
using CommandLine.Text;
using Octokit;
using Octokit.Internal;
using System.Globalization;

namespace SvnLogReporter
{
    public enum SourceControlType { GitHub, SVN };
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
            Options options = GetCommandLineOptions(args);
            Policy p = Policy.CreateFromFile(options.PolicyPath);
            options.LoadDates(p);

            var processor = Processors[p.SourceControl.Type](p, options);
            var report = processor.GenerateReport();
            Reporter.WriteReport(p, report, processor.PathToLog);

            var emailer = new ReportEmailer(p, options);
            emailer.TrySendEmails();
        }

        private static void SetAppConfigFile()
        {
            var currentConfigPath = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
            var newConfigPath = Path.Combine(Environment.CurrentDirectory, Path.GetFileName(currentConfigPath));
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", newConfigPath);
        }

        private static Options GetCommandLineOptions(string[] args)
        {
            Options options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }


        
    }
}
