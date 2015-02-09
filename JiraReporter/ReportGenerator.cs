using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
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
        public static JiraReport GenerateReport(JiraPolicy policy, JiraOptions options)
        {
            var timesheetService = new TimesheetService();
            var pullRequests = new List<JiraPullRequest>();
            var commits = new List<JiraCommit>();

            if (policy.SourceControlOptions != null)
            {
                var log = SourceControlProcessor.GetSourceControlLog(policy, options);
                pullRequests = SourceControlProcessor.GetPullRequests(log);
                commits = SourceControlProcessor.GetCommits(log);
            }

            return GetReport(policy, options, pullRequests, commits);
        }
        private static JiraReport GetReport(JiraPolicy policy, JiraOptions options, List<JiraPullRequest> pullRequests, List<JiraCommit> commits)
        {
            var sprintTasks = GetSprintReport(policy, options, pullRequests);
            var sprint = GenerateSprint(policy, options);

            var authors = GetReportAuthors(policy, options, pullRequests, commits, sprintTasks, sprint);

            var report = new JiraReport(policy, options)
            {
                Authors = authors,
                SprintTasks = sprintTasks,
                PullRequests = pullRequests,
                Date = options.FromDate,
                Summary = LoadSummary(policy, options, authors, sprintTasks, sprint, pullRequests)
            };
            report.Title = JiraReportHelpers.GetReportTitle(options, policy);

            return report;
        }

        private static List<JiraAuthor> GetReportAuthors(JiraPolicy policy, JiraOptions options, List<JiraPullRequest> pullRequests, List<JiraCommit> commits, SprintTasks sprintTasks, Sprint sprint)
        {
            var authors = new List<JiraAuthor>();
            var authorLoader = new AuthorLoader(options, policy, sprint, sprintTasks, commits, pullRequests);
            if (options.DraftKey != null)
            {
                var author = authorLoader.CreateAuthorByKey(options.DraftKey, policy);
                if (policy.GeneratedProperties.ProjectManager == author.Username)
                    author.IsProjectLead = true;
                authors.Add(author);
            }
            else
                authors = authorLoader.GetAuthors();

            return authors;
        }

        public static IndividualReport GetIndividualReport(JiraReport report, JiraAuthor author)
        {
            var individualReport = new IndividualReport(report.Policy, report.Options)
            {
                Summary = report.Summary,
                PullRequests = report.PullRequests,
                Date = report.Date,
                SprintTasks = report.SprintTasks,
                Author = author,
                Authors = report.Authors
            };
            individualReport.Title = JiraReportHelpers.GetReportTitle(report.Options, report.Policy, author.Name);

            return individualReport;
        }

        private static SprintTasks GetSprintReport(JiraPolicy policy, JiraOptions options, List<JiraPullRequest> pullRequests)
        {
            var tasksService = new TasksService();
            return tasksService.GetSprintTasks(policy, options, pullRequests);
        }

        public static Sprint GenerateSprint(JiraPolicy policy, JiraOptions options)
        {
            var jira = new JiraService(policy, options);
            return jira.GetLatestSprint();
        }

        public static Summary LoadSummary(JiraPolicy policy, JiraOptions options, List<JiraAuthor> authors, SprintTasks sprintTasks, Sprint sprint, List<JiraPullRequest> pullRequests)
        {
            var summaryLoader = new SummaryLoader(policy, options, authors, sprintTasks, sprint, pullRequests);
            return summaryLoader.LoadSummary();
        }
    }
}
