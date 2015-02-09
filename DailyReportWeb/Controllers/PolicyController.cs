using Atlassian.Connect.Jwt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DailyReportWeb.Controllers
{
    public class PolicyController : Controller
    {
        
        [JwtAuthentication]
        public ActionResult Index()
        {
            AuthenticationHelpers.SetAuthCookie(GetJiraUsername(), GetJiraBaseUrl());
            return View();
        }

        private string GetJiraBaseUrl()
        {
            var baserUrl = Request.QueryString["xdm_e"] + Request.QueryString["cp"];
            return baserUrl;
        }

        private string GetJiraUsername()
        {
            var userId = Request.QueryString["user_id"];
            return userId;
        }

        [Authorize]
        public ActionResult Details(long id)
        {
            return View((object)id);
        }
    }
}