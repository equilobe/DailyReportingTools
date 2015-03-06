﻿using Equilobe.DailyReport.Models.Storage;
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
using Equilobe.DailyReport.Utils;

namespace JiraReporter
{
    class Program
    {
        static void Main(string[] args)
        {

            var options = new JiraOptions();                
            new CommandLineParser().ParseArguments(args, options);

            var policy = new PolicyService().GetPolicy(options.UniqueProjectKey);

            var report = new JiraReport(policy, options);
            var reportService = new ReportService(report);
            reportService.SetReportFromDb();
            report.JiraRequestContext = JiraReportHelpers.GetJiraRequestContext(report);

            reportService.SetExecutionInstance();

            var project = new JiraService(report.JiraRequestContext).GetProject(report.Policy.ProjectId);
            SetProjectInfo(report, project);
            LoadReportDates(report);

            var contextService = new JiraContextService(report);
            contextService.SetPolicy();  
      
            if (RunReport(report))
                RunReportTool(report);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        private static bool RunReport(JiraReport context)
        {
            var today = DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date;

            if (context.LastReportSentDate.Date == today)
                return false;

            if (DatesHelper.IsWeekend(context))
                return false;

            if (CheckDayFromOverrides(context))
                return false;

            if (context.ExecutionInstance != null && context.ExecutionInstance.DateAdded.Date != DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Date)
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
            if (report.IsIndividualDraft)
            {
                var reportService = new ReportService(report.UniqueProjectKey);
                foreach (var author in report.Authors)
                {
                    var individualReport = ReportGenerator.GetIndividualReport(report, author);
                    reportService.SaveIndividualDraftConfirmation(author.IndividualDraftInfo);
                    var reportProcessor = new IndividualReportProcessor(individualReport);
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
            context.OffsetFromUtc = new TimeSpan(2,0,0);
            new DatesHelper(context).LoadDates();
        }

        private static void SetProjectInfo(JiraReport report, Project project)
        {
            report.ProjectName = project.Name;
            report.ProjectKey = project.Key;
            if (project.Lead != null)
                report.ProjectManager = project.Lead.key;
        }
    }
}
