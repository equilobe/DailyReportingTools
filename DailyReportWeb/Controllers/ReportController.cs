using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                var sendReport = RunReport(id);
                if (sendReport == true)
                    return Content("Report confirmed. Final report sent");

                return Content("Error in sending the final report");
            }

            return Content("Cannot confirm report for another date");
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date)
        {
            if (date.Date == DateTime.Today)
            {
                var resendReport = RunReport(id);
                if (resendReport == true)
                    return Content("Draft report was resent");

                return Content("Error in resending draft report");
            }

            return Content("Cannot resend draft for another date");
        }

        public bool RunReport(string id)
        {
            var key = "DRT-" + id;
            using (TaskService ts = new TaskService())
            {
                var task = ts.AllTasks.ToList().Find(t => t.Name == key);
                if (task != null)
                {
                    try
                    {
                        task.Run();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return false;
                // return string.Join(", ", ts.AllTasks.Select(t => t.Name));
            }
        }
    }
}