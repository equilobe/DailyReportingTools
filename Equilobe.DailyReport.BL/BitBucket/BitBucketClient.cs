using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Interfaces;
using RestSharp;

namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketClient
    {
        RestClient Client { get; set; }
        IConfigurationService ConfigurationService { get; set; }

        public static BitBucketClient CreateWithBasicAuth(string baseUrl, string username, string password)
        {
            return new BitBucketClient
            {
                Client = RestApiHelper.BasicAuthentication(baseUrl, username, password)
            };
        }

        protected BitBucketClient()
        {
        }

        public PullRequestPage GetPullRequests(string owner, string repository, string page = "1")
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequests(owner, repository, page), Method.GET);

            return RestApiHelper.ResolveRequest<PullRequestPage>(Client, request);
        }

        public CommitPage GetCommits(string owner, string repository, string page = "1")
        {
            var request = new RestRequest(BitBucketApiUrls.Commits(owner, repository, page), Method.GET);

            return RestApiHelper.ResolveRequest<CommitPage>(Client, request);
        }
    }
}
