using CommandLine;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
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
        public ISettingsService SettingsService { get; set; }
        public IReportExecutionService ReportExecutionService { get; set; }
        public IReportGeneratorService ReportGeneratorService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public ITimeZoneService TimeZoneService { get; set; }

        public void Execute(string[] args)
        {
            var options = new JiraOptions();
            new CommandLineParser().ParseArguments(args, options);

            var jiraRequestContext = new JiraRequestContext();
            long projectId;
            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Where(bs => bs.UniqueProjectKey == options.UniqueProjectKey).SingleOrDefault();
                if (basicSettings == null)
                    throw new ApplicationException("Unable to run report tool due to incorrect Project Key.");

                projectId = basicSettings.ProjectId;
                basicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);
            }

            SyncProject(options, jiraRequestContext, projectId);

            var policy = DataService.GetPolicy(options.UniqueProjectKey);
            var report = new JiraReport(policy, options);
            report.Settings = DataService.GetReportSettingsWithDetails(report.UniqueProjectKey);

            if (report.Settings.InstalledInstance.ExpirationDate <= DateTime.Now)
            {
                UpdateOnFailed(report, "Instance is inactive");
                throw new ApplicationException("Instance is inactive");
            }

            SetLastReportDatesFromSettings(report);
            report.JiraRequestContext = jiraRequestContext;
            LoadReportDates(report);
            SetExecutionInstance(report);

            var project = JiraService.GetProject(report.JiraRequestContext, report.Policy.ProjectId);
            SetProjectInfo(report, project);

            var contextService = new JiraContextService(report) { ConfigurationService = ConfigurationService };
            contextService.SetPolicy();

            if (RunReport(report))
                TryRunReport(report);
            else
            {
                UpdateOnFailed(report, "Unable to run report tool due to policy settings or final report already generated");
                throw new ApplicationException("Unable to run report tool due to policy settings or final report already generated.");
            }
        }

        void TryRunReport(JiraReport report)
        {
            try
            {
                RunReportTool(report);
                UpdateOnSucces(report);
            }
            catch (Exception ex)
            {
                UpdateOnFailed(report, "Failed. " + ex.Message + "." + ex.StackTrace);
            }
        }

        private void SyncProject(JiraOptions options, JiraRequestContext jiraRequestContext, long projectId)
        {
            try
            {
                var projects = JiraService.GetProjectsInfo(jiraRequestContext);
                if (!projects.IsEmpty() && projects.SingleOrDefault(p => p.ProjectId == projectId) != null)
                    return;

                using (var db = new ReportsDb())
                {
                    var basicSettings = db.BasicSettings.Where(bs => bs.UniqueProjectKey == options.UniqueProjectKey).Single();
                    db.BasicSettings.Remove(basicSettings);

                    db.SaveChanges();
                }

                TaskSchedulerService.DeleteTask(options.UniqueProjectKey);
            }
            catch (Exception ex)
            {           
                throw new ApplicationException("Request for JIRA projects failed", ex);
            }
        }

        private void SetLastReportDatesFromSettings(JiraReport _report)
        {
            if (_report.Settings.ReportExecutionSummary != null)
            {
                if (_report.Settings.ReportExecutionSummary.LastDraftSentDate != null)
                    _report.LastDraftSentDate = _report.Settings.ReportExecutionSummary.LastDraftSentDate.Value;
                if (_report.Settings.ReportExecutionSummary.LastFinalReportSentDate != null)
                    _report.LastReportSentDate = _report.Settings.ReportExecutionSummary.LastFinalReportSentDate.Value;
            }

            if (_report.Settings.FinalDraftConfirmation != null)
                if (_report.Settings.FinalDraftConfirmation.LastFinalDraftConfirmationDate != null)
                    _report.LastFinalDraftConfirmationDate = _report.Settings.FinalDraftConfirmation.LastFinalDraftConfirmationDate.Value;
        }

        ReportExecutionInstance GetUnexecutedInstance(BasicSettings report)
        {
            if (report.ReportExecutionInstances.IsEmpty())
                return null;

            var execInstance = report.ReportExecutionInstances.Where(qr => qr.DateExecuted == null).FirstOrDefault();
            return execInstance;
        }

        void SetExecutionInstance(JiraReport _report)
        {
            var executionContext = new ExecutionInstanceContext
            {
                BasicSettingsId = _report.Settings.Id,
                OffsetFromUtc = _report.OffsetFromUtc
            };

            ReportExecutionService.UpdateDeprecatedExecutionInstances(executionContext);

            var executionInstance = GetUnexecutedInstance(_report.Settings);
            _report.ExecutionInstance = new ExecutionInstance();
            if (executionInstance != null)
                executionInstance.CopyPropertiesOnObjects(_report.ExecutionInstance);
            else
            {
                _report.IsOnSchedule = true;
                ReportExecutionService.AddScheduledExecutionInstance(_report);
            }

            ReportExecutionService.MarkExecutionInstanceAsExecuted(new ItemContext(_report.ExecutionInstance.Id));
        }


        private bool RunReport(JiraReport context)
        {
            if (context.LastReportSentDate != null && context.LastReportSentDate != new DateTime() && DateTimeHelpers.CompareDay(context.LastReportSentDate, DateTime.Now, context.OffsetFromUtc) == 1)
                return false;

            if (DatesHelper.IsWeekend(context))
                return false;

            if (CheckDayFromOverrides(context))
                return false;

            return true;
        }

        private bool CheckDayFromOverrides(JiraReport context)
        {
            if (context.Policy.CurrentOverride != null && !string.IsNullOrEmpty(context.Policy.CurrentOverride.NonWorkingDays))
                return context.Policy.CurrentOverride.NonWorkingDaysList.Exists(a => a == DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc).Day);
            return false;
        }

        private void RunReportTool(JiraReport context)
        {
            SetTemplateGlobal();

            var report = ReportGeneratorService.GenerateReport(context);

            ProcessAndSendReport(report);
        }

        private void ProcessAndSendReport(JiraReport report)
        {
            if (report.IsIndividualDraft)
            {
                foreach (var author in report.Authors)
                {
                    var individualReport = ReportGeneratorService.GetIndividualReport(report, author);
                    var context = new UserConfirmationContext
                    {
                        Id = report.UniqueProjectKey,
                        DraftKey = author.IndividualDraftInfo.UniqueUserKey,
                        Info = author.IndividualDraftInfo,
                    };
                    ReportExecutionService.SaveIndividualDraftConfirmation(context);
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
            context.OffsetFromUtc = DataService.GetOffsetFromProjectKey(context.UniqueProjectKey);
            new DatesHelper(context).LoadDates();
        }

        private void SetProjectInfo(JiraReport report, Project project)
        {
            report.ProjectName = project.Name;
            report.ProjectKey = project.Key;
            if (project.Lead != null)
                report.ProjectManager = project.Lead.key;
        }

        void UpdateOnSucces(JiraReport report)
        {
            var executionInstanceContext = new ExecutionInstanceContext
            {
                Id = report.ExecutionInstance.Id,
                Status = "Succes"
            };

            ReportExecutionService.MarkExecutionInstanceStatus(executionInstanceContext);

            if (report.IsIndividualDraft)
                return;

            ReportExecutionService.MarkSentDates(report);
        }

        void UpdateOnFailed(JiraReport report, string status)
        {
            var context = new ExecutionInstanceContext { Id = report.ExecutionInstance.Id, Status = status };
            ReportExecutionService.MarkExecutionInstanceStatus(context);
        }

    }
}
