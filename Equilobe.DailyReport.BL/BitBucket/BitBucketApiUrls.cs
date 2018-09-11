namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketApiUrls
    {
        public static string PullRequests(string owner, string repository, int page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests?page={2}", owner, repository, page);
        }

        public static string UpdatedPullRequests(string owner, string repository, string updatedOn, int page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests?q=updated_on>{2}&page={3}", owner, repository, updatedOn, page);
        }

        public static string PullRequestComments(string owner, string repository, int pullRequestId, string createdOn, int page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests/{2}/comments?pagelen=100&page={3}&q=created_on>{4}", owner, repository, pullRequestId, page, createdOn);
        }

        public static string Commits(string owner, string repository, int page)
        {
            return string.Format("repositories/{0}/{1}/commits?page={2}", owner, repository, page);
        }

        public static string Contributors(string team, int page)
        {
            return string.Format("teams/{0}/members?page={1}", team, page);
        }
    }
}
