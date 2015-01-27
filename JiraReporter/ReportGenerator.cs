using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ReportGenerator
    {
        public static Report GenerateReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetService = new TimesheetService();
            var pullRequests = new List<PullRequest>();
            var commits = new List<Commit>();

            if (policy.SourceControlOptions != null)
            {
                var log = SourceControlProcessor.GetSourceControlLog(policy, options);
                pullRequests = SourceControlProcessor.GetPullRequests(log);
                commits = SourceControlProcessor.GetCommits(log);
            }

            return GetReport(policy, options, pullRequests, commits);
        }
        private static Report GetReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests, List<Commit> commits)
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

        private static List<Author> GetReportAuthors(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests, List<Commit> commits, SprintTasks sprintTasks, Sprint sprint)
        {
            var authors = new List<Author>();
            var authorLoader = new AuthorLoader(options, policy, sprint, sprintTasks, commits, pullRequests);
            if (options.DraftKey != null)
            {
                var author = authorLoader.GetAuthorByKey(options.DraftKey, policy);
                authors.Add(author);
            }
            else
                authors = authorLoader.GetAuthors();

            return authors;
        }

        public static IndividualReport GetIndividualReport(Report report, Author author)
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

        private static SprintTasks GetSprintReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(policy, options, pullRequests);
            return report;
        }

        public static Sprint GenerateSprint(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var jira = new JiraService(policy, options);
            return jira.GetLatestSprint();
        }
    }
}
