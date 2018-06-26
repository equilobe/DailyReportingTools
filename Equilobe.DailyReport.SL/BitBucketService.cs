using Equilobe.DailyReport.BL.BitBucket;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.SL
{
    public class BitBucketService : IBitBucketService
    {
        public void GetPullRequests(JiraPolicy policy)
        {
            var credentials = policy.SourceControlOptions.Credentials;
            var baseUrl = policy.BaseUrl;

            GetClient(credentials, baseUrl).GetPullRequests(policy.SourceControlOptions.RepoOwner, policy.SourceControlOptions.Repo);
        }

        private BitBucketClient GetClient(Credentials credentials, string baseUrl)
        {
            return BitBucketClient.CreateWithBasicAuth(baseUrl, credentials.Username, credentials.Password);
        }
    }
}
