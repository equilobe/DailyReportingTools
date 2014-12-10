using Microsoft.Win32.TaskScheduler;
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
            if (date.Date == DateTime.Today)
            {
                RunReport(id);
                return Content("Draft confirmed. Final report sent");
            }
            else
                return Content("Confirmation invalid");
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date)
        {
            return Content("key = " + id + " date = " + date);
        }

        public void RunReport(string id)
        {
            var key = "DRT-" + id;
            using (TaskService ts = new TaskService())
            {
                var task = ts.AllTasks.ToList().Find(t => t.Name == key);
                if (task != null)
                    task.Run();
            }
        }

    }
}