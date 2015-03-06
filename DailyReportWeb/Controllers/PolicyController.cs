using Equilobe.DailyReport.SL;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class PolicyController : Controller
    {
        [JwtAuthentication]
        public ActionResult Index()
        {
            AuthenticationHelpers.SetAuthCookie(new PolicyService().GetJiraUsername(Request.QueryString),
                                                new PolicyService().GetJiraBaseUrl(Request.QueryString));
            return View();
        }

        [Authorize]
        public ActionResult Details(long id)
        {
            return View((object)id);
        }
    }
}