using System.Collections.Generic;
using System.Web;

namespace DailyReportWeb.Controllers
{
    public class ConnectDescriptor
    {
        public ConnectDescriptor()
        {
            scopes = new List<string>();
            vendor = new ConnectDescriptorVendor();
        }

        public string name { get; set; }
        public string description { get; set; }
        public string key { get; set; }
        public string baseUrl { get; set; }
        public ConnectDescriptorVendor vendor { get; set; }
        public dynamic authentication { get; set; }
        public dynamic lifecycle { get; set; }
        public int? apiVersion { get; set; }
        public dynamic modules { get; set; }
        public IList<string> scopes { get; set; }
    }

    public class ConnectDescriptorVendor
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public static class ConnectDescriptorExtensions
    {
        public static void SetBaseUrlFromRequest(this ConnectDescriptor descriptor, HttpRequestBase request)
        {
            descriptor.baseUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') + "/";
        }
    }
}
