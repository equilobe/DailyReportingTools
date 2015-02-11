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
        private static JiraReport GetReport(JiraReport context, List<JiraPullRequest> pullRequests, List<JiraCommit> commits)
        {            
            var sprint = GenerateSprint(context);

            var sprintTasks = GetSprintReport(context, pullRequests);

            var authors = GetReportAuthors(context, pullRequests, commits, sprintTasks, sprint);
            
            var summary = LoadSummary(context, authors, sprintTasks, sprint, pullRequests);
            
            var report = new JiraReport(policy)
            {
                Authors = authors,
                SprintTasks = sprintTasks,
                PullRequests = pullRequests,
                Date = options.FromDate,
                Summary = summary,
                FromDate = options.FromDate,
                ToDate = options.ToDate
            };
            report.Title = JiraReportHelpers.GetReportTitle(report.FromDate, report.ToDate, policy);

            return report;
        }

        private static List<JiraAuthor> GetReportAuthors(JiraReport context, List<JiraPullRequest> pullRequests, List<JiraCommit> commits, SprintTasks sprintTasks, Sprint sprint)
        {
            var authors = new List<JiraAuthor>();
            var authorLoader = new AuthorLoader(context, sprint, sprintTasks, commits, pullRequests);
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


        private static SprintTasks GetSprintReport(JiraReport report)
        {
            var tasksService = new TasksService();
            return tasksService.GetSprintTasks(report);
        }

        public static Sprint GenerateSprint(JiraReport report)
        {
            var jira = new JiraService(report);
            return jira.GetLatestSprint();
        }

        public static Summary LoadSummary(JiraPolicy policy, JiraOptions options, List<JiraAuthor> authors, SprintTasks sprintTasks, Sprint sprint, List<JiraPullRequest> pullRequests)
        {
            var summaryLoader = new SummaryLoader(policy, options, authors, sprintTasks, sprint, pullRequests);
            return summaryLoader.LoadSummary();
        }
    }
}
