using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using JiraReporter.Services;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ReportGenerator
    {
        public static JiraReport GenerateReport(JiraReport report)
        {
            var timesheetService = new TimesheetService();
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
        private static JiraReport CompleteReport(JiraReport context)
        {
            context.Sprint = GenerateSprint(context);

            SetSprintReport(context);

            context.Authors = GetReportAuthors(context);

            context.Summary = LoadSummary(context);

            context.Title = JiraReportHelpers.GetReportTitle(context);

            return context;
        }

        private static List<JiraAuthor> GetReportAuthors(JiraReport context)
        {
            var authors = new List<JiraAuthor>();
            var authorLoader = new AuthorLoader(context);
            if (context.Options.DraftKey != null)
            {
                var author = authorLoader.CreateAuthorByKey(context.Options.DraftKey, context.Policy);
                if (context.Policy.GeneratedProperties.ProjectManager == author.Username)
                    author.IsProjectLead = true;
                authors.Add(author);
            }
            else
                authors = authorLoader.GetAuthors();

            return authors;
        }


        private static void SetSprintReport(JiraReport report)
        {
            var tasksService = new TasksService();
            tasksService.SetSprintTasks(report);
        }

        public static Sprint GenerateSprint(JiraReport report)
        {
            var jira = new SprintLoader(report);
            return jira.GetLatestSprint();
        }

        public static Summary LoadSummary(JiraReport report)
        {
            var summaryLoader = new SummaryLoader(report);
            return summaryLoader.LoadSummary();
        }
    }
}
