using Equilobe.DailyReport.JWT;
using Equilobe.DailyReport.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReportWeb.Helpers
{
    public class JwtAuthenticationAttribute : ActionFilterAttribute
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

                var sharedSecret = new DataService().GetSharedSecret(baseUrl);

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