using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public ActionResult Send(string id, DateTime date)
        {
            return Content("key = " + id + " date = " + date);
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date)
        {
            return Content("key = " + id + " date = " + date);
        }
    }
}