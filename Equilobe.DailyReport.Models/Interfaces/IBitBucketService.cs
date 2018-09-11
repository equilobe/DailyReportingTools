using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IBitBucketService : IService
    {
        Log GetLog(ISourceControlContext context);
        List<PullRequestComment> GetPullRequestComments(SourceControlOptions options, int pullRequestId, DateTime lastSync);
        List<PullRequest> GetPullRequests(SourceControlOptions sourceControlOptions, DateTime? lastSync);
        List<Commit> GetAllCommits(SourceControlOptions sourceControlOptions, DateTime fromDate, DateTime toDate);
        List<string> GetContributorsFromCommits(SourceControlContext context);
        List<string> GetAllContributors(SourceControlOptions sourceControlOptions);
    }
}
