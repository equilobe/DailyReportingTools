using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IBitBucketService : IService
    {
        void GetPullRequests(JiraPolicy policy);
    }
}
