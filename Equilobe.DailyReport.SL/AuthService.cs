using Equilobe.DailyReport.BL.Jira;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace Equilobe.DailyReport.SL
{
    public static class AuthService
    {
        public static void Authorize(this WebClient client, JiraPolicy policy, string relativeUrl)
        {
            if (!string.IsNullOrEmpty(policy.SharedSecret))
                client.Headers.Add("Authorization", "JWT " + JwtAuthenticator.CreateJwt(ConfigurationManager.AppSettings["addonKey"], policy.SharedSecret, relativeUrl, "GET"));
            else
                client.Headers.Add("Authorization", "Basic " + CreateBasic(policy.Username, policy.Password));
        }

        public static string CreateBasic(string username, string password)
        {
            var byteString = UTF8Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password));
            return Convert.ToBase64String(byteString);
        }
    }

    public class JwtAuthentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var requestToken = filterContext.HttpContext.Request.QueryString["jwt"];
                var baseUrl = filterContext.HttpContext.Request.QueryString["xdm_e"] + filterContext.HttpContext.Request.QueryString["cp"];

                if (String.IsNullOrEmpty(requestToken))
                {
                    throw new Exception("Authentication failed, missing JWT token");
                }

                if (String.IsNullOrEmpty(baseUrl))
                {
                    throw new Exception("Authentication failed, missing host and context from caller");
                }

                var sharedSecret = DbService.GetSharedSecret(baseUrl);

                var token = new EncodedJwtToken(sharedSecret, requestToken).Decode();
                token.ValidateToken(filterContext.HttpContext.Request);
            }
            catch (Exception)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}
