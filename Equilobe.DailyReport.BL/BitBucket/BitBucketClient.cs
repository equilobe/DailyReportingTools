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

        public void GetPullRequests(string owner, string repository)
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequests(owner, repository), Method.GET);

            var calumea = RestApiHelper.ResolveRequest<PullRequests>(Client, request);

            var checking = 2;
        }
    }
}
