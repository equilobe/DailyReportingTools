using Atlassian.Connect.Jwt;
using Equilobe.DailyReport.SL;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class PolicyController : Controller
    {
        [JwtAuthentication]
        public ActionResult Index()
        {
            AuthenticationHelpers.SetAuthCookie(PolicyService.GetJiraUsername(Request.QueryString),
                                                PolicyService.GetJiraBaseUrl(Request.QueryString));
            return View();
        }

        [Authorize]
        public ActionResult Details(long id)
        {
            return View((object)id);
        }
    }
}