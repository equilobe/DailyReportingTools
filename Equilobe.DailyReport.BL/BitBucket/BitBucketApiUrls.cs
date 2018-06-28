namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketApiUrls
    {
        public static string PullRequests(string owner, string repository, int page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests?page={2}", owner, repository, page);
        }

        public static string Commits(string owner, string repository, int page)
        {
            return string.Format("repositories/{0}/{1}/commits?page={2}", owner, repository, page);
        }
    }
}
