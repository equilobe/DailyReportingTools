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
namespace JiraReporter
{
    class ReportGeneratorService : IReportGeneratorService
    {
        public IJiraService JiraService { get; set; }
        public IEncryptionService EncryptionService { get; set; }
        public IConfigurationService ConfigurationService { get; set; }

        public JiraReport GenerateReport(JiraReport report)
        {
            SetSourceControlLogs(report);

            report.Sprint = GenerateSprint(report);

            SetReportTasks(report);

            report.Authors = GetReportAuthors(report);

            report.Summary = LoadSummary(report);

            report.Title = JiraReportHelpers.GetReportTitle(report);

            return report;
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
            var authorLoader = new AuthorLoader(context) { JiraService = JiraService, EncryptionService = EncryptionService, ConfigurationService = ConfigurationService };
            if (context.ExecutionInstance != null && !string.IsNullOrEmpty(context.ExecutionInstance.UniqueUserKey) && context.ExecutionInstance.Scope == SendScope.SendIndividualDraft)
            {
                var author = authorLoader.CreateAuthorByKey(context);
                if (context.ProjectManager == author.Username)
                    author.IsProjectLead = true;
                authors.Add(author);
            }
            else
                authors = authorLoader.GetAuthors();

            return authors;
        }

        public JiraReport GetIndividualReport(JiraReport report, JiraAuthor author)
        {
            var individualReport = new JiraReport(report.Policy, report.Options)
            {
                Author = author,
                Summary = report.Summary,
                OffsetFromUtc = report.OffsetFromUtc,
                PullRequests = report.PullRequests,
                Sprint = report.Sprint,
                ReportTasks = report.ReportTasks,
                Commits = report.Commits,
                JiraRequestContext = report.JiraRequestContext,
                ProjectName = report.ProjectName,
                IsFinalDraft = report.IsFinalDraft,
                IsIndividualDraft = report.IsIndividualDraft,
                IsFinalReport = report.IsFinalReport,
                UniqueProjectKey = report.UniqueProjectKey,
                HasSprint = report.HasSprint
            };
            individualReport.Title = JiraReportHelpers.GetReportTitle(individualReport, true);

            return individualReport;
        }


        private void SetReportTasks(JiraReport report)
        {
            var tasksService = new TaskLoader() { JiraService = JiraService };
            tasksService.SetReportTasks(report);
        }

        Sprint GenerateSprint(JiraReport report)
        {
            var projectDateFilter = new ProjectDateFilter { Context = report.JiraRequestContext, Date = report.FromDate, ProjectKey = report.ProjectKey, ProjectName = report.ProjectName };
            var sprint = JiraService.GetProjectSprintForDate(projectDateFilter);
            if (sprint != null)
                report.HasSprint = true;

            return sprint;
        }

        Summary LoadSummary(JiraReport report)
        {
            var summaryLoader = new SummaryLoader(report);
            return summaryLoader.LoadSummary();
        }
    }
}
