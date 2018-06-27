﻿namespace Equilobe.DailyReport.BL.BitBucket
{
    public class BitBucketApiUrls
    {
        public static string PullRequests(string owner, string repository, string page)
        {
            return string.Format("repositories/{0}/{1}/pullrequests?page={2}", owner, repository, page);
        }
    }
}
