using Equilobe.DailyReport.Models.Storage;
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
using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Services;
using JiraReporter.Helpers;
using CommandLine;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.SL;

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

            var project = new JiraService().GetProject(report.Settings, report.Policy.ProjectId.ToString());
            SetProjectInfo(policy, project);
            LoadReportDates(report);
            var policyService = new JiraPolicyService(report);
            policyService.SetPolicy();        

            if (RunReport(report))
                RunReportTool(report);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        private static bool RunReport(JiraReport context)
        {
            var today = DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date;

            if (context.Policy.GeneratedProperties.LastReportSentDate.Date == DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date)
                return false;

            if (DatesHelper.IsWeekend(context))
                return false;

            if (CheckDayFromOverrides(context))
                return false;

            if (context.Options.TriggerKey != null && !context.Policy.IsForcedByLead(context.Options.TriggerKey))
                return false;

            return true;
        }

        private static bool CheckDayFromOverrides(JiraReport context)
        {
            if (context.Policy.CurrentOverride != null && context.Policy.CurrentOverride.NonWorkingDays != null)
                return context.Policy.CurrentOverride.NonWorkingDaysList.Exists(a => a == DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Day);
            return false;
        }

        private static void RunReportTool(JiraReport context)
        {
            SetTemplateGlobal();

            var report = ReportGenerator.GenerateReport(context);

            ProcessAndSendReport(report);
        }

        private static void ProcessAndSendReport(JiraReport report)
        {
            if (report.Policy.GeneratedProperties.IsIndividualDraft)
            {
                foreach (var author in report.Authors)
                {
                    report.Author = author;
                    report.Title = JiraReportHelpers.GetReportTitle(report);
                    var reportProcessor = new IndividualReportProcessor(report);
                    reportProcessor.ProcessReport();
                }
            }
            else
            {
                var reportProcessor = new BaseReportProcessor(report);
                reportProcessor.ProcessReport();
            }
        }

        private static void SetTemplateGlobal()
        {
            Razor.SetTemplateBase(typeof(SourceControlLogReporter.RazorEngine.ExtendedTemplateBase<>));
        }



        private static void LoadReportDates(JiraReport context)
        {
            var timesheetSample = new JiraService().GetTimesheet(context.Settings, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
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
