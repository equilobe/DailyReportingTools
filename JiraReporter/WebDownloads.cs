using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class WebDownloads
    {
        public static Image ImageFromURL(string Url, string username, string password)
        {

            var webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "image/png");
            webClient = AuthorizeClient(username, password, webClient);

            var imageData = webClient.DownloadData(Url);

            MemoryStream stream = new MemoryStream(imageData);
            var img = Image.FromStream(stream);
            stream.Close();

            return img;
        }

        public static WebClient AuthorizeClient(string username, string password, WebClient client)
        {
            var credentials = string.Format("{0}:{1}", username, password);
            var credentialsBase64 = GetBase64String(credentials);

            client.Headers.Add("Authorization", "Basic " + credentialsBase64);
            return client;
        }

        public static string GetBase64String (string randomString)
        {
            var byteString = UTF8Encoding.UTF8.GetBytes(randomString);
            return Convert.ToBase64String(byteString);
        }
       
    }
}
