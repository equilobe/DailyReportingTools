using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using JiraReporter.Services;
using System;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public ActionResult SendReport(string id, DateTime date)
        {
            var reportService = new ReportService(id);
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            if (!reportService.CanSendFullDraft())
                return Content("Not all individual drafts were confirmed");

            reportService.SetFinalDraftConfirmation();
            reportService.SetReportExecutionInstance(SendScope.SendFinalDraft);

            if (new TaskSchedulerService(id).TryRunReportTask())
                return Content("Report confirmed. Final report sent");
            else
                return Content("Error in sending the final report");
        }

        [HttpGet]
        public ActionResult SendDraft(string id, DateTime date, string draftKey="")
        {
            var reportService = new ReportService(id);

            if (date.Date != DateTime.Today)
                return Content("Cannot resend draft for another date");

            if (!reportService.CanSendFullDraft(draftKey))
                return Content("Cannot send report if not all individual drafts were confirmed");

            reportService.SetReportExecutionInstance(SendScope.SendFinalDraft);

            if (new TaskSchedulerService(id).TryRunReportTask())
                return Content("Draft report was resent");
            else
                return Content("Error in sending draft report");
        }

        [HttpGet]
        public ActionResult ConfirmIndividualDraft(string id, DateTime date, string draftKey)
        {
            var reportService = new ReportService(id);

            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            if (reportService.CheckIndividualConfirmation(draftKey))
                return Content("This draft is already confirmed");

            if (!reportService.ConfirmIndividualDraft(draftKey)) 
                return Content("Error in confirmation");

            if (reportService.CanSendFullDraft(id))
            {
                reportService.SetReportExecutionInstance(SendScope.SendFinalDraft);
                if (!new TaskSchedulerService(id).TryRunReportTask())
                    return Content("Report confirmed. Error in sending full draft report");

                return Content("Report confirmed. Full draft sent");
            }

            return Content("Report confirmed");
        }

        [HttpGet]
        public ActionResult SendIndividualDraft(string id, DateTime date, string draftKey)
        {
            var reportService = new ReportService(id);

            if (date.Date != DateTime.Today)
                return Content("Cannot resend report for another date");

            if (reportService.CheckIndividualConfirmation(draftKey))
                return Content("Draft is already confirmed. Can't resend");

            reportService.SetReportExecutionInstance(SendScope.SendIndividualDraft, draftKey);
            if (!new TaskSchedulerService(id).TryRunReportTask())
                return Content("Error in sending individual draft");

            return Content("Report resent");
        }
    }
}