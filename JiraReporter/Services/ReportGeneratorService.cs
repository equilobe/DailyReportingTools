using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using JiraReporter.Services;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira.Filters;
using Equilobe.DailyReport.SL;
using JiraReporter.Helpers;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Autofac;
using Equilobe.DailyReport.Utils;
namespace JiraReporter
{
    class ReportGeneratorService : IReportGeneratorService
    {
        public IJiraService JiraService { get; set; }
        public IEncryptionService EncryptionService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }
        public IDataService DataService { get; set; }
        public IErrorService ErrorService { get; set; }

        public JiraReport GenerateReport(JiraReport report)
        {
            SetWorkingDaysContext(report);

            FilterIndividualDrafts(report);

            SetSourceControlLogs(report);

            SetReportSprintDetails(report);

            SetReportTasks(report);

            report.Authors = GetReportAuthors(report);

            report.Summary = LoadSummary(report);

            SetDetails(report);

            report.NotHideContentId = RandomString.Get();

            return report;
        }

        private void SetWorkingDaysContext(JiraReport report)
        {
            report.WorkingDaysContext = new WorkingDaysContext(report.Policy.MonthlyOptions, report.Policy.AdvancedOptions.WeekendDaysList);
        }

        private static void SetDetails(JiraReport report)
        {
            report.Title = JiraReportHelpers.GetReportTitle(report);
            report.Date = ReportDateFormatter.GetReportDate(report.FromDate, report.ToDate);

            if (!report.IsIndividualDraft)
                report.EmailSubject = JiraReportHelpers.GetReportSubject(report);
        }

        private static void SetSourceControlLogs(JiraReport report)
        {
            report.PullRequests = new List<JiraPullRequest>();
            report.Commits = new List<JiraCommit>();

            if (report.Policy.SourceControlOptions != null && report.Policy.SourceControlOptions.Type != SourceControlType.None)
            {
                var log = SourceControlProcessor.GetSourceControlLog(report);
                report.PullRequests = SourceControlProcessor.GetPullRequests(log);
                report.Commits = SourceControlProcessor.GetCommits(log);
            }
        }

        private List<JiraAuthor> GetReportAuthors(JiraReport context)
        {
            var authors = new List<JiraAuthor>();
            var authorLoader = new AuthorLoader(context)
            {
                JiraService = JiraService,
                EncryptionService = EncryptionService,
                ConfigurationService = ConfigurationService,
                DataService = DataService,
                ErrorService = ErrorService
            };

            if (context.ExecutionInstance != null
                && !string.IsNullOrEmpty(context.ExecutionInstance.UniqueUserKey)
                && context.ExecutionInstance.Scope == SendScope.SendIndividualDraft)
            {
                var author = authorLoader.CreateAuthorByKey(context);
                if (context.ProjectManager == author.UserKey)
                    author.IsProjectLead = true;
                authors.Add(author);
            }
            else
                authors = authorLoader.GetAuthors();

            return authors;
        }

        public JiraReport GetIndividualReport(JiraReport report, JiraAuthor author)
        {
            var individualReport = new JiraReport(report.Policy, report.Options);
            report.CopyPropertiesOnObjects(individualReport);
            individualReport.Author = author;

            individualReport.Title = JiraReportHelpers.GetReportTitle(individualReport, true);
            individualReport.EmailSubject = JiraReportHelpers.GetReportSubject(individualReport);

            return individualReport;
        }


        private void SetReportTasks(JiraReport report)
        {
            var tasksService = new TaskLoader() { JiraService = JiraService };
            tasksService.SetReportTasks(report);
        }

        void SetReportSprintDetails(JiraReport report)
        {
            var projectDateFilter = new ProjectDateFilter { Context = report.JiraRequestContext, Date = report.FromDate, ProjectKey = report.ProjectKey, ProjectName = report.ProjectName };

            var sprintContext = JiraService.GetProjectSprintDetailsForDate(projectDateFilter);

            if (sprintContext == null)
                return;

            report.PastSprint = sprintContext.PastSprint;
            report.FutureSprint = sprintContext.FutureSprint;
            report.Sprint = sprintContext.ReportSprint;
        }

        Summary LoadSummary(JiraReport report)
        {
            var summaryLoader = new SummaryLoader(report) { ErrorService = ErrorService };
            return summaryLoader.LoadSummary();
        }

        void FilterIndividualDrafts(JiraReport report)
        {
            report.Settings.IndividualDraftConfirmations = report.Settings.IndividualDraftConfirmations.Where(r => r.ReportDate == report.ToDate.DateToString()).ToArray();
        }
    }
}
