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
using Equilobe.DailyReport.Models.Interfaces;
using Autofac;
namespace JiraReporter
{
    class ReportGeneratorService : IReportGeneratorService
    {
        public IJiraService JiraService { get; set; }

        public JiraReport GenerateReport(JiraReport report)
        {
            report.PullRequests = new List<JiraPullRequest>();
            report.Commits = new List<JiraCommit>();

            if (report.Policy.SourceControlOptions != null)
            {
                var log = SourceControlProcessor.GetSourceControlLog(report);
                report.PullRequests = SourceControlProcessor.GetPullRequests(log);
                report.Commits = SourceControlProcessor.GetCommits(log);
            }

            return CompleteReport(report);
        }
        private JiraReport CompleteReport(JiraReport context)
        {
            context.Sprint = GenerateSprint(context);

            SetSprintReport(context);

            context.Authors = GetReportAuthors(context);

            context.Summary = LoadSummary(context);

            context.Title = JiraReportHelpers.GetReportTitle(context);

            return context;
        }

        private List<JiraAuthor> GetReportAuthors(JiraReport context)
        {
            var authors = new List<JiraAuthor>();
            var authorLoader = new AuthorLoader(context) { JiraService = JiraService };
            if (context.ExecutionInstance != null && !string.IsNullOrEmpty(context.ExecutionInstance.UniqueUserKey))
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
                SprintTasks = report.SprintTasks,
                Commits = report.Commits,
                JiraRequestContext = report.JiraRequestContext,
                ProjectName = report.ProjectName,
                IsFinalDraft = report.IsFinalDraft,
                IsIndividualDraft = report.IsIndividualDraft,
                IsFinalReport = report.IsFinalReport,
                UniqueProjectKey = report.UniqueProjectKey
            };
            individualReport.Title = JiraReportHelpers.GetReportTitle(individualReport, true);

            return individualReport;
        }


        private void SetSprintReport(JiraReport report)
        {
            var tasksService = new TaskLoader() { JiraService = JiraService };
            tasksService.SetSprintTasks(report);
        }

        Sprint GenerateSprint(JiraReport report)
        {
            var projectDateFilter = new ProjectDateFilter { Context = report.JiraRequestContext, Date = report.FromDate, ProjectKey = report.ProjectKey, ProjectName = report.ProjectName };
            return JiraService.GetProjectSprintForDate(projectDateFilter);
        }

        Summary LoadSummary(JiraReport report)
        {
            var summaryLoader = new SummaryLoader(report);
            return summaryLoader.LoadSummary();
        }
    }
}
