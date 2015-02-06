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
        public static Report GenerateReport(JiraPolicy policy, JiraOptions options)
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
        private static Report GetReport(JiraPolicy policy, JiraOptions options, List<JiraPullRequest> pullRequests, List<JiraCommit> commits)
        {
            var sprintTasks = GetSprintReport(policy, options, pullRequests);
            var sprint = GenerateSprint(policy, options);

            var authors = GetReportAuthors(policy, options, pullRequests, commits, sprintTasks, sprint);

            var report = new Report(policy, options)
            {
                Authors = authors,
                SprintTasks = sprintTasks,
                PullRequests = pullRequests,
                Date = options.FromDate,
                Summary = new Summary(authors, sprintTasks, pullRequests, policy, options, sprint)
            };
            report.Title = report.GetReportTitle();

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

        public static IndividualReport GetIndividualReport(Report report, JiraAuthor author)
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
            individualReport.Title = individualReport.GetReportTitle();

            return individualReport;
        }

        private static SprintTasks GetSprintReport(JiraPolicy policy, JiraOptions options, List<JiraPullRequest> pullRequests)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(policy, options, pullRequests);
            return report;
        }

        public static Sprint GenerateSprint(JiraPolicy policy, JiraOptions options)
        {
            var jira = new JiraService(policy, options);
            return jira.GetLatestSprint();
        }
    }
}
