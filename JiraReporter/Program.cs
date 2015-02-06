using CommandLine;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using RazorEngine;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SourceControlLogReporter;
using Equilobe.DailyReport.Models.Jira;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            JiraOptions options = GetCommandLineOptions(args);
            JiraPolicy policy = JiraPolicyService.LoadFromFile(options.PolicyPath);
            var project = RestApiRequests.GetProject(policy);
            SetProjectInfo(policy, project);
            var policyService = new JiraPolicyService(policy);
            policyService.SetPolicy(options);

            LoadReportDates(policy, options);

            if (RunReport(policy, options))
                RunReportTool(policy, options);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        private static bool RunReport(JiraPolicy policy, JiraOptions options)
        {
            var today = DateTime.Now.ToOriginalTimeZone().Date;

            if (policy.GeneratedProperties.LastReportSentDate.Date == DateTime.Now.ToOriginalTimeZone().Date)
                return false;

            if (options.IsWeekend() == true)
                return false;

            if (CheckDayFromOverrides(policy) == true)
                return false;

            if (options.TriggerKey != null && !policy.IsForcedByLead(options.TriggerKey))
                return false;

            return true;
        }

        private static bool CheckDayFromOverrides(JiraPolicy policy)
        {
            if (policy.CurrentOverride != null && policy.CurrentOverride.NonWorkingDays != null)
                return policy.CurrentOverride.NonWorkingDaysList.Exists(a => a == DateTime.Now.ToOriginalTimeZone().Day);
            return false;
        }

        private static void RunReportTool(JiraPolicy policy, JiraOptions options)
        {
            SetTemplateGlobal();

            var report = ReportGenerator.GenerateReport(policy, options);

            ProcessAndSendReport(policy, options, report);
        }

        private static void ProcessAndSendReport(JiraPolicy policy, JiraOptions options, JiraReport report)
        {
            if (policy.GeneratedProperties.IsIndividualDraft)
            {
                foreach (var author in report.Authors)
                {
                    var individualReport = ReportGenerator.GetIndividualReport(report, author);
                    var reportProcessor = new IndividualReportProcessor(policy, options);
                    reportProcessor.ProcessReport(individualReport);
                }
        //        policy.SaveToFile(options.PolicyPath);
            }
            else
            {
                var reportProcessor = new BaseReportProcessor(policy, options);
                reportProcessor.ProcessReport(report);
            }
        }

        private static void SetTemplateGlobal()
        {
            Razor.SetTemplateBase(typeof(SourceControlLogReporter.RazorEngine.ExtendedTemplateBase<>));
        }

        private static JiraOptions GetCommandLineOptions(string[] args)
        {
            JiraOptions options = new JiraOptions();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }

        private static void LoadReportDates(JiraPolicy policy, JiraOptions options)
        {
            var timesheetSample = RestApiRequests.GetTimesheet(policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(timesheetSample.StartDate);
            options.LoadDates(policy);
        }

        private static void SetProjectInfo(JiraPolicy policy, Project project)
        {
            policy.GeneratedProperties.ProjectName = project.Name;
            policy.GeneratedProperties.ProjectKey = project.Key;
            if (project.Lead != null)
                policy.GeneratedProperties.ProjectManager = project.Lead.key;
        }
    }
}
