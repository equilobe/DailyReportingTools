using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Interfaces;
using RestSharp;
using System.Linq;

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

        public BitBucketResponsePage<PullRequest> GetPullRequests(string owner, string repository, string updatedOn, int page)
        {
            var request = updatedOn == null ?
                new RestRequest(BitBucketApiUrls.PullRequests(owner, repository, page), Method.GET) :
                new RestRequest(BitBucketApiUrls.UpdatedPullRequests(owner, repository, updatedOn, page));

            return RestApiHelper.ResolveRequest<BitBucketResponsePage<PullRequest>>(Client, request);
        }

        public BitBucketResponsePage<Commit> GetCommits(string owner, string repository, int page = 1)
        {
            var request = new RestRequest(BitBucketApiUrls.Commits(owner, repository, page), Method.GET);

            return RestApiHelper.ResolveRequest<BitBucketResponsePage<Commit>>(Client, request);
        }

        public BitBucketResponsePage<Contributor> GetContributors(string team, int page = 1)
        {
            var request = new RestRequest(BitBucketApiUrls.Contributors(team, page), Method.GET);

            return RestApiHelper.ResolveRequest<BitBucketResponsePage<Contributor>>(Client, request);
        }

        public BitBucketResponsePage<PullRequestComment> GetPullRequestComments(string owner, string repository, int pullRequestId, string createdOn, int page)
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequestComments(owner, repository, pullRequestId, createdOn, page));

            return RestApiHelper.ResolveRequest<BitBucketResponsePage<PullRequestComment>>(Client, request);
        }
    }
}
