using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using Equilobe.DailyReport.Models.Web;

namespace DailyReportWeb
{
    public static class AuthenticationHelpers
    {
        private const int CookieVersion = 1; 


        public static void SetUser()
        {
            var user = GetUser();
            SetUser(user);
        }

        private static WebPrincipal GetUser()
        {
            if (IsAnonymous())
                return WebPrincipal.GetAnonymous();

            var identity = HttpContext.Current.User.Identity;
            var baseUrl = GetBaseUrlFromCookie();

            return new WebPrincipal(identity, baseUrl);
        }

        private static void SetUser(WebPrincipal principal)
        {
            Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = Thread.CurrentPrincipal;
        }

        private static bool IsAnonymous()
        {
            var user = HttpContext.Current.User;

            return user == null ||
                user.Identity == null ||
                user.Identity.IsAuthenticated == false ||
                GetCookieVersion() != CookieVersion;
        }


        private static string GetBaseUrlFromCookie()
        {
            return GetAuthTicket().UserData;
        }

        private static int GetCookieVersion()
        {
            return GetAuthTicket().Version;
        }

        private static FormsAuthenticationTicket GetAuthTicket()
        {
            var request = HttpContext.Current.Request;
            var authCookie = request.Cookies.Get(FormsAuthentication.FormsCookieName);
            return FormsAuthentication.Decrypt(authCookie.Value);
        }

        public static void SetAuthCookie(string userName, string baserUrl, bool rememberMe = true)
        {
            SetAuthCookie(userName, baserUrl, GetExpirationDate(rememberMe));
        }

        private static DateTime GetExpirationDate(bool rememberMe)
        {
            if (rememberMe)
                return DateTime.Now.Add(new TimeSpan(14, 0, 0, 0));

            return DateTime.Now.Add(FormsAuthentication.Timeout);
        }

        private static void SetAuthCookie(string userName, string baseUrl, DateTime expirationDate)
        {
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName)
            {
                Value = GetEncryptedAuthTicket(userName, baseUrl, expirationDate),
                Expires = expirationDate
            };

            HttpContext.Current.Response.Cookies.Add(cookie);
        }        
        
        private static string GetEncryptedAuthTicket(string userName, string baseUrl, DateTime expirationDate)
        {
            var ticket = new FormsAuthenticationTicket(
                                version: CookieVersion,
                                name: userName,
                                issueDate: DateTime.Now,
                                expiration: expirationDate,
                                isPersistent: true, 
                                userData: baseUrl);

            return FormsAuthentication.Encrypt(ticket);
        }

        public static string GetBaseUrl(this IPrincipal webPrincipal)
        {
            return ((WebPrincipal)webPrincipal).BaseUrl;
        }

        public static string GetUsername(this IPrincipal webPrincipal)
        {
            return ((WebPrincipal)webPrincipal).Identity.Name;
        }
    }
}
