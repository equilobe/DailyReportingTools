using CommandLine;
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

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            SourceControlLogReporter.Options options = GetCommandLineOptions(args);
            SourceControlLogReporter.Model.Policy policy = SourceControlLogReporter.Model.Policy.CreateFromFile(options.PolicyPath);
            var project = RestApiRequests.GetProject(policy);
            SetProjectInfo(policy, project);
            policy.SetDefaultProperties();
            LoadReportDates(policy, options);

            if (RunReport(policy, options))
                RunReportTool(args, policy, options);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        private static bool RunReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var today = DateTime.Now.ToOriginalTimeZone().Date;

            if (policy.GeneratedProperties.LastReportSentDate.Date == DateTime.Now.ToOriginalTimeZone().Date)
                return false;

            if (options.IsWeekend() == true)
                return false;

            if (CheckDayFromOverrides(policy) == true)
                return false;

            return true;
        }

        private static bool CheckDayFromOverrides(SourceControlLogReporter.Model.Policy policy)
        {
            if (policy.CurrentOverride != null && policy.CurrentOverride.NonWorkingDays != null)
                return policy.CurrentOverride.NonWorkingDaysList.Exists(a => a == DateTime.Now.ToOriginalTimeZone().Day);
            return false;
        }

        private static void RunReportTool(string[] args, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            SetTemplateGlobal();

            var report = ReportGenerator.GenerateReport(policy, options);

            ProcessAndSendReport(policy, options, report);
        }

        private static void ProcessAndSendReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, Report report)
        {
            if (policy.GeneratedProperties.IsIndividualDraft)
            {
                foreach (var author in report.Authors)
                {
                    var individualReport = ReportGenerator.GetIndividualReport(report, author);
                    var reportProcessor = new IndividualReportProcessor(policy, options);
                    reportProcessor.ProcessReport(individualReport);
                }
                policy.SaveToFile(options.PolicyPath);
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

        private static SourceControlLogReporter.Options GetCommandLineOptions(string[] args)
        {
            SourceControlLogReporter.Options options = new SourceControlLogReporter.Options();
            ICommandLineParser parser = new CommandLineParser();
            parser.ParseArguments(args, options);
            return options;
        }

        private static void LoadReportDates(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetSample = RestApiRequests.GetTimesheet(policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1), string.Empty);
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(timesheetSample.StartDate);
            options.LoadDates(policy);
        }

        private static void SetProjectInfo(SourceControlLogReporter.Model.Policy policy, JiraModels.Project project)
        {
            policy.GeneratedProperties.ProjectName = project.Name;
            policy.GeneratedProperties.ProjectKey = project.Key;
        }
    }
}
