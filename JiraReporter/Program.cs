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
using JiraReporter.Services;
using JiraReporter.Helpers;
using CommandLine;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new JiraOptions();                
            new CommandLineParser().ParseArguments(args, options);
            var policy = JiraPolicyService.LoadFromFile(options.PolicyPath);

            var report = new JiraReport(policy, options);

            var project = RestApiRequests.GetProject(policy);
            SetProjectInfo(policy, project);
            var policyService = new JiraPolicyService(policy);
            policyService.SetPolicy(options);

            LoadReportDates(report);

            if (RunReport(report))
                RunReportTool(policy, options);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        private static bool RunReport(JiraReport context)
        {
            var today = DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date;

            if (context.Policy.GeneratedProperties.LastReportSentDate.Date == DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date)
                return false;

            if (DatesHelper.IsWeekend(context) == true)
                return false;

            if (CheckDayFromOverrides(context.Policy) == true)
                return false;

            if (context.Options.TriggerKey != null && !context.Policy.IsForcedByLead(context.Options.TriggerKey))
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
                    report.Author = author;
                    report.Title = JiraReportHelpers.GetReportTitle(report.FromDate, report.ToDate, report.Policy, author.Name);
                    var reportProcessor = new IndividualReportProcessor(report);
                    reportProcessor.ProcessReport();
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



        private static void LoadReportDates(JiraReport context)
        {
            var timesheetSample = RestApiRequests.GetTimesheet(context.Policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
            context.OffsetFromUtc = JiraOffsetHelper.GetOriginalTimeZoneFromDateAtMidnight(timesheetSample.StartDate);
            new DatesHelper(context).LoadDates();
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
