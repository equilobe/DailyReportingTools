using Equilobe.DailyReport.JWT;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL.Jira
{
    public class WebDownloads
    {    
        public static byte[] GetUserAvatar(AuthorizationContext context, string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "image/png");
            Authorize(webClient, context, UrlExtensions.GetRelativeUrl(url));

            return webClient.DownloadData(url);
        }

        static void Authorize(WebClient client, AuthorizationContext context, string relativeUrl)
        {
            if (!string.IsNullOrEmpty(context.SharedSecret))
                client.Headers.Add("Authorization", "JWT " + JwtAuthenticator.CreateJwt(context.AddonKey, context.SharedSecret, relativeUrl, "GET"));
            else
                client.Headers.Add("Authorization", "Basic " + CreateBasic(context.Username, context.Password));
        }

        static string CreateBasic(string username, string password)
        {
            var byteString = UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return Convert.ToBase64String(byteString);
        }

    }
}
