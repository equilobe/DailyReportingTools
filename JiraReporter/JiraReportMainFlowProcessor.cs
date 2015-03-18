﻿using CommandLine;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using JiraReporter.Helpers;
using JiraReporter.Model;
using JiraReporter.Services;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class JiraReportMainFlowProcessor
    {
        public IDataService DataService { get; set; }
        public IJiraService JiraService { get; set; }
        public IPolicyService PolicyService { get; set; }

        public void Execute(string[] args)
        {
            DependencyInjection.Init();

            var options = new JiraOptions();
            new CommandLineParser().ParseArguments(args, options);

            var policy = PolicyService.GetPolicy(options.UniqueProjectKey);

            var report = new JiraReport(policy, options);
            DataService.SetReportFromDb(report);
            report.JiraRequestContext = JiraReportHelpers.GetJiraRequestContext(report);

            SetExecutionInstance(report);


            var project = JiraService.GetProject(report.Policy.ProjectId);
            SetProjectInfo(report, project);
            LoadReportDates(report);

            var contextService = new JiraContextService(report);
            contextService.SetPolicy();

            if (RunReport(report))
                RunReportTool(report);
            else
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
        }

        ReportExecutionInstance GetUnexecutedInstance(ReportSettings report)
        {
            var execInstance = report.ReportExecutionInstances.Where(qr => qr.DateExecuted == null).FirstOrDefault();
            return execInstance;
        }

        public void SetExecutionInstance(JiraReport _report)
        {
            var unexecutedInstance = GetUnexecutedInstance(_report.Settings);
            if (unexecutedInstance != null)
            {
                _report.ExecutionInstance = new ExecutionInstance();
                unexecutedInstance.CopyPropertiesOnObjects(_report.ExecutionInstance);
                new ReportExecutionService().MarkExecutionInstanceAsExecuted(new ItemContext { Id = unexecutedInstance.Id });
            }
            else
                _report.IsOnSchedule = true;
        }

        private bool RunReport(JiraReport context)
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

        private bool CheckDayFromOverrides(JiraReport context)
        {
            if (context.Policy.CurrentOverride != null && context.Policy.CurrentOverride.NonWorkingDays != null)
                return context.Policy.CurrentOverride.NonWorkingDaysList.Exists(a => a == DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Day);
            return false;
        }

        private void RunReportTool(JiraReport context)
        {
            SetTemplateGlobal();

            var report = new ReportGenerator().GenerateReport(context);

            ProcessAndSendReport(report);
        }

        private void ProcessAndSendReport(JiraReport report)
        {
            if (report.IsIndividualDraft)
            {
                var reportService = new ReportExecutionService();
                foreach (var author in report.Authors)
                {
                    var individualReport = new ReportGenerator().GetIndividualReport(report, author);
                    var context = new UserConfirmationContext
                    {
                        Id = report.UniqueProjectKey,
                        DraftKey = author.IndividualDraftInfo.UserKey,
                        Info = author.IndividualDraftInfo,
                    };
                    reportService.SaveIndividualDraftConfirmation(context);
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

        private void SetTemplateGlobal()
        {
            Razor.SetTemplateBase(typeof(SourceControlLogReporter.RazorEngine.ExtendedTemplateBase<>));
        }

        private void LoadReportDates(JiraReport context)
        {
            context.OffsetFromUtc = new TimeSpan(2, 0, 0);
            new DatesHelper(context).LoadDates();
        }

        private void SetProjectInfo(JiraReport report, Project project)
        {
            report.ProjectName = project.Name;
            report.ProjectKey = project.Key;
            if (project.Lead != null)
                report.ProjectManager = project.Lead.key;
        }
    }
}
