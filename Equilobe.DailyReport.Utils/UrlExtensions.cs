using System;

namespace System.Net.Http
{
    public class UrlExtensions
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
