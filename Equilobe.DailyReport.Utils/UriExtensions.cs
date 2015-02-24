using System;

namespace Equilobe.DailyReport.Utils
{
    public class UriExtensions
    {
        public static string GetHostUrl(string url)
        {
            return new Uri(url).GetLeftPart(UriPartial.Authority);
        }

        public static string GetAbsoluteUrl(string url)
        {
            return new Uri(url).GetLeftPart(UriPartial.Path);
        }

        public static string GetRelativeUrl(string url)
        {
            return new Uri(url).PathAndQuery.Substring(1);
        }
    }
}
