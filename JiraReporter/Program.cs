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
            LoadReportDates(policy, options);
            policy.SetPermanentTaskLabel();

            if (RunReport(policy, options))
                RunReportTool(args, policy, options);
            else
                throw new JiraException("\r\nUnable to run report tool due to policy settings or final report already generated.\r\n");
        }

        private static bool RunReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var today = DateTime.Now.ToOriginalTimeZone().Date;

            if (policy.LastReportSentDate.Date == DateTime.Now.ToOriginalTimeZone().Date)
                return false;

            if (policy.IsWeekendReportActive == true && options.IsWeekend() == true)
                return false;

            if (CheckDayFromOverrides(policy) == true)
                return false;

            return true;
        }

        private static bool CheckDayFromOverrides(SourceControlLogReporter.Model.Policy policy)
        {
            if (policy.CurrentOverride != null && policy.CurrentOverride.NonWorkingDays != null)
                return policy.CurrentOverride.NonWorkingDays.Exists(a => a == DateTime.Now.ToOriginalTimeZone().Day);
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
            if (policy.AdvancedOptions.NoIndividualDraft)
            {
                var reportProcessor = new BaseReportProcessor(policy, options);
                reportProcessor.ProcessReport(report);
            }
            else
            {
                foreach (var author in report.Authors)
                {
                    var individualReport = ReportGenerator.GetIndividualReport(report, author);
                    var reportProcessor = new IndividualReportProcessor(policy, options);
                    reportProcessor.ProcessReport(individualReport);
                }
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
            var timesheetSample = RestApiRequests.GetTimesheet(policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(timesheetSample.StartDate);
            options.LoadDates(policy);
        }
    }

    [Serializable()]
    public class JiraException : Exception
    {
        public JiraException() : base() { }
        public JiraException(string message) : base(message) { }
        public JiraException(string message, Exception inner) : base(message, inner) { }
    }
}
