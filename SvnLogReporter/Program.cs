﻿using CommandLine;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Models.Storage;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace SourceControlLogReporter
{
    class Program
    {
        private static readonly Dictionary<SourceControlType, Func<Policy, Options, ReportBase>> Processors = new Dictionary<SourceControlType, Func<Policy, Options, ReportBase>>()
        {
            {SourceControlType.GitHub, ReportBase.Create<GitHubReport>},
            {SourceControlType.SVN, ReportBase.Create<SvnReport>},
            {SourceControlType.Bitbucket, ReportBase.Create<BitBucketReport>}
        };

        static void Main(string[] args)
        {
            try
            {
                SetGlobalSettings();
                DependencyInjection.Init();
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
            Policy policy = SourceControlPolicyService.LoadFromFile(options.PolicyPath);
            var policyService = new SourceControlPolicyService(policy);
            policyService.SetPolicy();
            options.LoadDates();

            if (policy.SourceControlOptions.Type == SourceControlType.None)
                return;

            var processor = Processors[policy.SourceControlOptions.Type](policy, options);
            var report = processor.GenerateReport();
            Reporter.WriteReport(policy, report, processor.PathToLog);

            var emailer = new ReportEmailer(policy, options);
            emailer.SendEmails();
        }

        private static void SetAppConfigFile()
        {
            var currentConfigPath = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
            var newConfigPath = Path.Combine(Environment.CurrentDirectory, Path.GetFileName(currentConfigPath));
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", newConfigPath);
        }
    }
}
