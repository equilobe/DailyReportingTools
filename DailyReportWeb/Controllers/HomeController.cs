using Equilobe.DailyReport.Utils;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            AuthHelpers.PluginAuth();
            return View(new RegexValidation());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}