using Equilobe.DailyReport.BL.GitHub;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Octokit;
using System.Collections.Generic;
namespace Equilobe.DailyReport.SL
{
    public class GitHubService : IGitHubService
    {
        private GithubClient GetClient(ISourceControlRequestContext context)
        {
            return new GithubClient(context.Username, context.Password);
        }

        public List<GitHubCommit> GetBranchCommits(ISourceControlRequestContext context, string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch)
        {
            return GetClient(context).GetBranchCommits(repositoryOwner, repositoryName, sinceDate, untilDate, branch);
        }

        public List<GitHubCommit> GetAllCommits(ISourceControlRequestContext context, string owner, string name, string sinceDate, string untilDate)
        {
            return GetClient(context).GetAllCommits(owner, name, sinceDate, untilDate);
        }

        public List<Branch> GetBranches(ISourceControlRequestContext context, string owner, string name)
        {
            return GetClient(context).GetBranches(owner, name);
        }

        public List<Octokit.User> GetAllContributors(ISourceControlRequestContext context, string owner, string name)
        {
            return GetClient(context).GetAllContributors(owner, name);
        }

        public Octokit.User GetUser(ISourceControlRequestContext context, string username)
        {
            return GetClient(context).GetUser(username);
        }

        public List<PullRequest> GetPullRequests(ISourceControlRequestContext context, string owner, string name)
        {
            return GetClient(context).GetPullRequests(owner, name);
        }

        public Log GetLog(ISourceControlContext context)
        {
            var client = GetClient(context.SourceControlOptions.Credentials);
            return new LogLoader(client, context).CreateLog();
        }
    }
}
