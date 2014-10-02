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
        public static  Report GenerateReport(SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options)
        {
            var pullRequests = new List<PullRequest>();
            GetPullRequests(pullRequests, policy, options);

            var timesheet = RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1));
            var timesheetService = new TimesheetService();
            timesheetService.SetTimesheetIssues(timesheet, policy, options, pullRequests);

            return GetReport(timesheet, policy, options, pullRequests);
        }
        private static  Report GetReport(Timesheet timesheet, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options, List<PullRequest> pullRequests)
        {            
            var sprint = GetSprintReport(policy, options, timesheet);
            var authors = AuthorsProcessing.GetAuthors(timesheet, sprint, policy, options, pullRequests);
            var report = new Report(policy, options)
            {
                Authors = authors,
                Sprint = sprint,
                Date = options.FromDate,
                Summary = new Summary(authors, sprint),
                Title = policy.ReportTitle,
                PullRequests = pullRequests
            };
                         
            return report;
        }

        private static SprintTasks GetSprintReport(SvnLogReporter.Model.Policy p, SvnLogReporter.Options options, Timesheet timesheet)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(p, timesheet, options);
            return report;
        }

        private static void GetPullRequests(List<PullRequest> pullRequests, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options)
        {
            var pullRequestsOct = new List<Octokit.PullRequest>();
            if (SvnLogReporter.SourceControlType.GitHub == policy.SourceControl.Type)
            {
                pullRequestsOct = SourceControlProcessor.GetPullRequests(policy, options);
                AddPullRequests(pullRequests, pullRequestsOct);
            }
        }

        private static void AddPullRequests(List<PullRequest> pullRequests, List<Octokit.PullRequest> pullRequestsOct)
        {
            if (pullRequestsOct.Count > 0)
                foreach (var pullRequest in pullRequestsOct)
                    pullRequests.Add(new PullRequest { GithubPullRequest = pullRequest });
        }

    }
}
