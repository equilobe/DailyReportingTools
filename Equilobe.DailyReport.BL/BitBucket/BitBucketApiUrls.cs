namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketApiUrls
    {
        public static string PullRequests(string owner, string repository)
        {
            return string.Format("repositories/{0}/{1}/pullrequests", owner, repository);
        }
    }
}
