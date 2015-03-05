using Equilobe.DailyReport.SL;
using JiraReporter.Services;
using System;
using System.Web.Mvc;

namespace DailyReportWeb.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public ActionResult Send(string id, DateTime date)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            var policy = JiraPolicyService.LoadPolicy(id);
            var policyPath = JiraPolicyService.GetPolicyPath(id);

            if (!policy.CanSendFullDraft())
                return Content("Not all individual drafts were confirmed");

            JiraPolicyService.SetPolicyFinalReport(policy, policyPath);

            if (TaskSchedulerService.TryRunReportTask(id))
                return Content("Report confirmed. Final report sent");
            else
                return Content("Error in sending the final report");
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date, string draftKey="")
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot resend draft for another date");

            var policy = JiraPolicyService.LoadPolicy(id);
            var policyPath = JiraPolicyService.GetPolicyPath(id);

            if (!policy.CanSendFullDraft(draftKey))
                return Content("Cannot send report if not all individual drafts were confirmed");

            if (!string.IsNullOrEmpty(draftKey))
            {
                TaskSchedulerService.RunReportDirectly(id, draftKey, true);
                return Content("Draft report was sent");
            }

            if (TaskSchedulerService.TryRunReportTask(id))
                return Content("Draft report was resent");
            else
                return Content("Error in sending draft report");
        }

        [HttpGet]
        public ActionResult ConfirmIndividualDraft(string id, DateTime date, string draftKey)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            var policyPath = JiraPolicyService.GetPolicyPath(id);
            var policy = JiraPolicyService.LoadFromFile(policyPath);

            var confirm = JiraPolicyService.SetIndividualDraftConfirmation(policy, draftKey, policyPath);
            if (!confirm)
                return Content("Error in confirmation");

            if (policy.CanSendFullDraft())
            {
                if (!TaskSchedulerService.TryRunReportTask(id))
                    return Content("Report confirmed. Error in sending full draft report");

                return Content("Report confirmed. Full draft sent");
            }

            return Content("Report confirmed");
        }

        [HttpGet]
        public ActionResult SendIndividualDraft(string id, DateTime date, string draftKey)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot resend report for another date");

            var policy = JiraPolicyService.LoadPolicy(id);

            if (policy.CheckIndividualDraftConfirmation(draftKey))
                return Content("Draft is already confirmed. Can't resend");

            TaskSchedulerService.RunReportDirectly(id, draftKey);

            return Content("Report resent");
        }
    }
}