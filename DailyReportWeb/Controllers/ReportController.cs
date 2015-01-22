using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DailyReportWeb.Helpers;
using SourceControlLogReporter.Model;
using System.Configuration;

namespace DailyReportWeb.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public ActionResult Send(string id, DateTime date)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            if (ReportRunner.TryRunReport(id))
                return Content("Report confirmed. Final report sent");
            else 
                return Content("Error in sending the final report");
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot resend draft for another date");

            if (ReportRunner.TryRunReport(id + "draft"))
                return Content("Draft report was resent");
            else 
                return Content("Error in resending draft report");
        }

        //Testing purpose
        [HttpGet]
        public ActionResult SendIndividualDraft(string id, DateTime date, string userKey) 
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            return Content("Report confirmed");
        }
    }
}