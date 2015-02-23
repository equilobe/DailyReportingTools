using System.Security.Principal;

namespace Equilobe.DailyReport.Models.Web
{
    public class WebPrincipal : IPrincipal
    {
        private WebPrincipal() { }

        public WebPrincipal(IIdentity identity, string baseUrl)
        {
            this.Identity = identity;
            this.BaseUrl = baseUrl;
        }

        public static WebPrincipal GetAnonymous()
        {
            return new WebPrincipal { Identity = new GenericIdentity("") };
        }

        public IIdentity Identity { get; private set; }

        public string BaseUrl { get; private set;  }


        public bool IsInRole(string role)
        {
            return true;
        }
    }
}