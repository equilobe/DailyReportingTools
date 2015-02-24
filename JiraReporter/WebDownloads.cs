using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;

namespace JiraReporter
{
    public static class WebDownloads
    {
        public static Image ImageFromURL(JiraPolicy policy, string url)
        {
            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "image/png");
            webClient.AuthorizeClient(policy, UriExtensions.GetRelativeUrl(url));

            var imageData = webClient.DownloadData(url);

            MemoryStream stream = new MemoryStream(imageData);
            var img = Image.FromStream(stream);
            stream.Close();

            return img;
        }

        public static void AuthorizeClient(this WebClient client, JiraPolicy policy, string relativeUrl)
        {
            if (!string.IsNullOrEmpty(policy.SharedSecret))
                client.Headers.Add("Authorization", "JWT " + JwtAuthenticator.CreateJwt(ConfigurationManager.AppSettings["addonKey"], policy.SharedSecret, relativeUrl, "GET"));
            else
                client.Headers.Add("Authorization", "Basic " + GetBasicAuthentification(policy.Username, policy.Password));
        }

        public static string GetBasicAuthentification(string username, string password)
        {
            var byteString = UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return Convert.ToBase64String(byteString);
        }
    }
}
