using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Policy;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IBitBucketService : IService
    {
        List<PullRequest> GetAllPullRequests(SourceControlOptions sourceControlOptions);
    }
}
