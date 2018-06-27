using Equilobe.DailyReport.BL.BitBucket;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.SL
{
    public class BitBucketService : IBitBucketService
    {
        public IConfigurationService ConfigurationService { get; set; }

        public void GetPullRequests(SourceControlOptions sourceControlOptions)
        {
            var credentials = sourceControlOptions.Credentials;
            var baseUrl = GetBaseUrl();

            var client = GetClient(credentials, baseUrl);

            client.GetPullRequests(sourceControlOptions.RepoOwner, sourceControlOptions.Repo);
        }

        private BitBucketClient GetClient(Credentials credentials, string baseUrl)
        {
            return BitBucketClient.CreateWithBasicAuth(baseUrl, credentials.Username, credentials.Password);
        }

        private string GetBaseUrl()
        {
            return ConfigurationService.GetBitBucketApiClientUrl();
        }
    }
}
