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

        public BitBucketResponsePage<PullRequest> GetPullRequests(string owner, string repository, int page = 1)
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequests(owner, repository, page), Method.GET);

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

        public BitBucketResponsePage<PullRequestComment> GetPullRequestComments(string owner, string repository, int pullRequestId)
        {
            var request = new RestRequest(BitBucketApiUrls.PullRequestComments(owner, repository, pullRequestId));

            return RestApiHelper.ResolveJiraRequest<BitBucketResponsePage<PullRequestComment>>(Client, request);
        }
    }
}
