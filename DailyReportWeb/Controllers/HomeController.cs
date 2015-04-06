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
    }
}