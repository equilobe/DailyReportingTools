using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.SL;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class ReportController : Controller
    {

        public IReportExecutionService ReportExecutionService { get; set; }

        [HttpGet]
        public ActionResult SendReport(ExecutionContext context)
        {
            var result = ReportExecutionService.SendReport(context);
            return Content(result.Message);
        }

        [HttpGet]
        public ActionResult SendDraft(ExecutionContext context)
        {
            var result = ReportExecutionService.SendDraft(context);
            return Content(result.Message);
        }

        [HttpGet]
        public ActionResult ConfirmIndividualDraft(ExecutionContext context)
        {
            var result = ReportExecutionService.ConfirmIndividualDraft(context);
            return Content(result.Message);
        }


        [HttpGet]
        public ActionResult SendIndividualDraft(ExecutionContext context)
        {
            var result = ReportExecutionService.SendIndividualDraft(context);
            return Content(result.Message);
        }
    }
}