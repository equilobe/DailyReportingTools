using Equilobe.DailyReport.BL.BitBucket;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using System.Collections.Generic;

namespace Equilobe.DailyReport.SL
{
    public class BitBucketService : IBitBucketService
    {
        public IConfigurationService ConfigurationService { get; set; }

        public List<PullRequest> GetAllPullRequests(SourceControlOptions sourceControlOptions)
        {
            var credentials = sourceControlOptions.Credentials;
            var baseUrl = GetBaseUrl();
            var client = GetClient(credentials, baseUrl);
            var pullRequests = new List<PullRequest>();
            var page = 1;

            while (true)
            {
                var pullRequestPage = client.GetPullRequests(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, page.ToString());

                if (pullRequestPage.Values != null)
                    pullRequests.AddRange(pullRequestPage.Values);

                if (pullRequestPage.Next == null)
                    break;

                page++;
            }

            return pullRequests;
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
