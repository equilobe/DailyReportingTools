using Equilobe.DailyReport.BL.GitHub;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Services;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class GitHubService : IGitHubService
    {
        private GithubClient GetClient(IRequestContext context)
        {
            return new GithubClient(context.Username, context.Password);
        }

        public List<GitHubCommit> GetBranchCommits(IRequestContext context, string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch)
        {
            return GetClient(context).GetBranchCommits(repositoryOwner, repositoryName, sinceDate, untilDate, branch);
        }

        public List<GitHubCommit> GetAllCommits(IRequestContext context, string owner, string name, string sinceDate, string untilDate)
        {
            return GetClient(context).GetAllCommits(owner, name, sinceDate, untilDate);
        }

        public List<Branch> GetBranches(IRequestContext context, string owner, string name)
        {
            return GetClient(context).GetBranches(owner, name);
        }

        public List<Octokit.User> GetAllContributors(IRequestContext context, string owner, string name)
        {
            return GetClient(context).GetAllContributors(owner, name);
        }

        public Octokit.User GetUser(IRequestContext context, string username)
        {
            return GetClient(context).GetUser(username);
        }

        public List<PullRequest> GetPullRequests(IRequestContext context, string owner, string name)
        {
            return GetClient(context).GetPullRequests(owner, name);
        }
    }
}
