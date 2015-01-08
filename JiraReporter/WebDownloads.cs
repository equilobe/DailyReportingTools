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
            byte[] imageData = DownloadData(Url, username, password);

            MemoryStream stream = new MemoryStream(imageData);
            var img = Image.FromStream(stream);
            stream.Close();


            return img;
        }

        private static byte[] DownloadData(string Url, string username, string password)
        {
            byte[] downloadedData = new byte[0];
            try
            {
                WebRequest req = WebRequest.Create(Url);
                req.Credentials = new NetworkCredential(username, password);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();

                byte[] buffer = new byte[1024];

                int dataLength = (int)response.ContentLength;

                MemoryStream memStream = new MemoryStream();
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }
                    else
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }
                }

                downloadedData = memStream.ToArray();

                stream.Close();
                memStream.Close();
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return downloadedData;
        }
    }
}
