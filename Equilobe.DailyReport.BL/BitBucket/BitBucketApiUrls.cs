namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketApiUrls
    {
        public static string PullRequests(string owner, string repository, int page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests?page={2}", owner, repository, page);
        }

        public static string PullRequestComments(string owner, string repository, int pullRequestId)
        {
            return string.Format("repositories/{0}/{1}/pullRequests/{2}/comments?pagelen=1000", owner, repository, pullRequestId);
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
