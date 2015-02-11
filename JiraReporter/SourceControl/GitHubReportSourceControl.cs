using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.ReportPolicy;
using Octokit;
using Octokit.Internal;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Jira;

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
            var pullRequests = GetPullRequests(Policy.SourceControlOptions.RepoOwner, Policy.SourceControlOptions.RepoName).ToList();
            var commits = GetReportCommits();
            return LoadLog(commits, pullRequests);
        }

        public override IReadOnlyList<PullRequest> GetPullRequests(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControlOptions.Username, Policy.SourceControlOptions.Password))));
            return connectionAll.GetAll<PullRequest>(ApiUrls.PullRequests(owner, name)).Result;
        }

        protected override List<GitHubCommit> GetReportCommits()
        {
            string fromDate = JiraOptions.DateToISO(Options.FromDate.ToOriginalTimeZone(Context.OffsetFromUtc.Negate()));
            string toDate = JiraOptions.DateToISO(Options.ToDate.ToOriginalTimeZone(Context.OffsetFromUtc.Negate()));
            return GetReportCommits(fromDate, toDate);
        }

        protected override IReadOnlyList<GitHubCommit> GetAllCommits(string owner, string name, string sinceDate, string untilDate, string branch)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControlOptions.Username, Policy.SourceControlOptions.Password))));
            return connectionAll.GetAll<GitHubCommit>(ApiUrls.RepositoryCommitsBranchDate(owner, name, sinceDate, untilDate, branch)).Result;
        }

        protected override IReadOnlyList<Branch> GetBranches(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControlOptions.Username, Policy.SourceControlOptions.Password))));
            return connectionAll.GetAll<Branch>(ApiUrls.RepoBranches(owner, name)).Result;
        }

        protected override List<GitHubCommit> ConcatCommits(string owner, string name, string sinceDate, string untilDate)
        {
            var branches = GetBranches(owner, name);
            var commits = new List<GitHubCommit>();
            var newCommits = new List<GitHubCommit>(); ;
            foreach (var branch in branches)
                commits = commits.Concat(GetAllCommits(owner, name, sinceDate, untilDate, branch.Name)).ToList();
            return commits;
        }

        public override IReadOnlyList<Octokit.User> GetAllContributors(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControlOptions.Username, Policy.SourceControlOptions.Password))));
            return connectionAll.GetAll<Octokit.User>(ApiUrls.RepositoryContributors(owner, name)).Result;
        }

        public override Octokit.User GetUserInfo(string username)
        {
            Ensure.ArgumentNotNullOrEmptyString(username, "username");
            ApiConnection connection = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControlOptions.Username, Policy.SourceControlOptions.Password))));
            return connection.Get<Octokit.User>(ApiUrls.User(username)).Result;
        }

        protected override Log LoadLog(List<GitHubCommit> commits, List<PullRequest> pullRequests)
        {
            var log = new Log();
            if (pullRequests != null)
            {
                log.PullRequests = pullRequests;
                SetPullRequestsAuthors(pullRequests);
            }
            log.Entries = new List<LogEntry>();
            foreach (var commit in commits)
            {
                log.Entries.Add(
                 new LogEntry
                 {
                     Author = commit.Commit.Author.Name,
                     Date = commit.Commit.Author.Date,
                     Message = commit.Commit.Message,
                     Revision = commit.Sha,
                     Link = commit.HtmlUrl
                 });
            }

            LogService.RemoveWrongEntries(Options.FromDate, log);
            return log;
        }
    }
}
