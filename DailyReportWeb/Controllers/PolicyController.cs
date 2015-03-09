using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
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

            ViewBag.Time = Validations.TIME;
            return View();
        }

        [Authorize]
        public ActionResult Details(long id)
        {
            ViewBag.ProjectId = id;
            ViewBag.Time = Validations.TIME;
            ViewBag.Mail = Validations.MAIL;
            ViewBag.Mails = Validations.MAILS;
            ViewBag.Digits = Validations.DIGITS;
            ViewBag.Days = Validations.DAYS;
            ViewBag.Url = Validations.URL;

            return View();
        }
    }
}