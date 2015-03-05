using Octokit;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IGitHubService
    {
        List<GitHubCommit> GetBranchCommits(ISourceControlRequestContext context, string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch);
        List<GitHubCommit> GetAllCommits(ISourceControlRequestContext context, string owner, string name, string sinceDate, string untilDate);
        List<Branch> GetBranches(ISourceControlRequestContext context, string owner, string name);
        List<Octokit.User> GetAllContributors(ISourceControlRequestContext context, string owner, string name);
        Octokit.User GetUser(ISourceControlRequestContext context, string username);
        Log GetLog(ISourceControlContext context);
    }
}
