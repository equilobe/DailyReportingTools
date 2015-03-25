using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.SL;
using System.Collections.Specialized;
using Equilobe.DailyReport.JWT;

namespace DailyReportWeb
{
    public static class AuthHelpers
    {
        private const int CookieVersion = 1;

        public static void PluginAuth()
        {
            var request = new HttpRequestWrapper(HttpContext.Current.Request);
            if (!IsPlugin(request))
                return;

            var userName = request.QueryString["user_id"];
            var baseUrl = request.QueryString["xdm_e"] + request.QueryString["cp"];
            SetAuthCookie(userName, baseUrl);
        }

        public static void SetAuthCookie(string userName, string baseUrl, bool rememberMe = true)
        {
            SetAuthCookie(userName, baseUrl, GetExpirationDate(rememberMe));
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

        private static DateTime GetExpirationDate(bool rememberMe)
        {
            if (rememberMe)
                return DateTime.Now.Add(new TimeSpan(14, 0, 0, 0));

            return DateTime.Now.Add(FormsAuthentication.Timeout);
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

        private static FormsAuthenticationTicket GetAuthTicket()
        {
            var request = HttpContext.Current.Request;
            var authCookie = request.Cookies.Get(FormsAuthentication.FormsCookieName);
            try
            {
                return FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetBaseUrl(this IPrincipal webPrincipal)
        {
            var authTicket = GetAuthTicket();

            if (authTicket != null)
                return authTicket.UserData;
            return null;
        }

        public static string GetUsername(this IPrincipal webPrincipal)
        {
            return webPrincipal.Identity.Name;
        }

        public static bool IsPlugin(this IPrincipal webPrincipal)
        {
            return !string.IsNullOrEmpty(webPrincipal.GetBaseUrl());
        }

        public static bool IsPlugin(HttpRequestBase request)
        {
            try
            {
                var requestToken = request.QueryString["jwt"];
                if (string.IsNullOrEmpty(requestToken))
                    return false;

                var baseUrl =  request.QueryString["xdm_e"] + request.QueryString["cp"];
                if (string.IsNullOrEmpty(baseUrl))
                    return false;

                var sharedSecret = new DataService().GetSharedSecret(baseUrl);
                var token = new EncodedJwtToken(sharedSecret, requestToken).Decode();
                token.ValidateToken(request);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
