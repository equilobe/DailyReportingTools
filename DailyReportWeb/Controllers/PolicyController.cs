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
            return string.Empty;
        }

        private string GetJiraUsername()
        {
            return string.Empty;
        }

        [Authorize]
        public ActionResult Details()
        {
            var baseUrl = User.GetBaseUrl();
            return View();
        }
    }
}