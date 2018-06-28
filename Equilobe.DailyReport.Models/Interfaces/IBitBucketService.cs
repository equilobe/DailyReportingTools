using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Policy;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IBitBucketService : IService
    {
        Log GetLog(ISourceControlContext context);
        List<PullRequest> GetAllPullRequests(SourceControlOptions sourceControlOptions);
        List<Commit> GetAllCommits(SourceControlOptions sourceControlOptions, DateTime fromDate, DateTime toDate);
    }
}
