using Equilobe.DailyReport.Models.ReportFrame;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Services
{
    public interface IGitHubService
    {
        List<GitHubCommit> GetBranchCommits(IRequestContext context, string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch);
        List<GitHubCommit> GetAllCommits(IRequestContext context, string owner, string name, string sinceDate, string untilDate);
        List<Branch> GetBranches(IRequestContext context, string owner, string name);
        List<Octokit.User> GetAllContributors(IRequestContext context, string owner, string name);
        Octokit.User GetUser(IRequestContext context, string username);
        Log GetLog(ISourceControlContext context);
    }
}
