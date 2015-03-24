using Octokit;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IGitHubService : IService
    {
        List<GitHubCommit> GetBranchCommits(ICredentials context, string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch);
        List<GitHubCommit> GetAllCommits(ICredentials context, string owner, string name, string sinceDate, string untilDate);
        List<Branch> GetBranches(ICredentials context, string owner, string name);
        List<Octokit.User> GetAllContributors(ICredentials context, string owner, string name);
        Octokit.User GetUser(ICredentials context, string username);
        Log GetLog(ISourceControlContext context);
    }
}
