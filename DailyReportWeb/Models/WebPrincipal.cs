using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace DailyReportWeb.Models
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
            return false;
        }
    }
}