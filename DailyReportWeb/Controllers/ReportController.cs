using DailyReportWeb.Helpers;
using Equilobe.DailyReport.Models.Storage;
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

            var policy = JiraContextService.LoadPolicy(id);
            var policyPath = JiraContextService.GetPolicyPath(id);

            if (!policy.CanSendFullDraft())
                return Content("Not all individual drafts were confirmed");

          // TODO: set final report mode
          //  JiraContextService.SetPolicyFinalReport(policy, policyPath);

            if (ReportRunner.TryRunReportTask(id))
                return Content("Report confirmed. Final report sent");
            else
                return Content("Error in sending the final report");
        }

        [HttpGet]
        public ActionResult ResendDraft(string id, DateTime date, string draftKey="")
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot resend draft for another date");

            var policy = JiraContextService.LoadPolicy(id);
            var policyPath = JiraContextService.GetPolicyPath(id);

            if (!policy.CanSendFullDraft(draftKey))
                return Content("Cannot send report if not all individual drafts were confirmed");

            if (!string.IsNullOrEmpty(draftKey))
            {
                ReportRunner.RunReportDirectly(id, draftKey, true);
                return Content("Draft report was sent");
            }

            if (ReportRunner.TryRunReportTask(id))
                return Content("Draft report was resent");
            else
                return Content("Error in sending draft report");
        }

        [HttpGet]
        public ActionResult ConfirmIndividualDraft(string id, DateTime date, string draftKey)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            var policyPath = JiraContextService.GetPolicyPath(id);
       //    var policy = JiraPolicyService.LoadFromFile(policyPath);
            var policy = new JiraPolicy();

            //TODO: use db to store confirmations
         //   var confirm = JiraContextService.SetIndividualDraftConfirmation(policy, draftKey);
            var confirm = false;
            if (!confirm)
                return Content("Error in confirmation");

            if (policy.CanSendFullDraft())
            {
                if (!ReportRunner.TryRunReportTask(id))
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

            var policy = JiraContextService.LoadPolicy(id);

            if (policy.CheckIndividualDraftConfirmation(draftKey))
                return Content("Draft is already confirmed. Can't resend");

            ReportRunner.RunReportDirectly(id, draftKey);

            return Content("Report resent");
        }
    }
}