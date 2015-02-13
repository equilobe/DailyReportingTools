using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Storage;
using Octokit;
using Octokit.Internal;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.SL;

namespace JiraReporter.SourceControl
{
    class GitHubReportSourceControl : GitHubReport
    {
        JiraPolicy Policy { get { return Context.Policy; } }
        JiraOptions Options { get { return Context.Options; } }
        JiraReport Context { get; set; }

        public GitHubReportSourceControl(JiraReport context)
        {
            Context = context;
        }

        public GitHubReportSourceControl()
        {

        }

        public override Log CreateLog()
        {
            var pullRequests = new GitHubService().GetPullRequests(Policy.SourceControlOptions.Credentials, Policy.SourceControlOptions.RepoOwner, Policy.SourceControlOptions.RepoName);
            var commits = GetReportCommits();
            return LoadLog(commits, pullRequests, Options.FromDate);
        }


        protected override List<GitHubCommit> GetReportCommits()
        {
            string fromDate = JiraOptions.DateToISO(Options.FromDate);
            string toDate = JiraOptions.DateToISO(Options.ToDate);
            return new GitHubService().GetAllCommits(Policy.SourceControlOptions.Credentials, Policy.SourceControlOptions.RepoOwner, Policy.SourceControlOptions.RepoName, fromDate, toDate);
        }
    }
}
