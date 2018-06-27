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

        public PullRequests GetPullRequests(string owner, string repository, string page = "1")
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequests(owner, repository, page), Method.GET);

            return RestApiHelper.ResolveRequest<PullRequests>(Client, request);
        }
    }
}
