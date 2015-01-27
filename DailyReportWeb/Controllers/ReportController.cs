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
        public ActionResult SendIndividualDraft(string id, DateTime date, string draftKey)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot confirm report for another date");

            var policyPath = ReportRunner.GetPolicyPath(id);
            var policy = Policy.CreateFromFile(policyPath);
            var confirm = ReportRunner.SetIndividualDraftConfirmation(draftKey, policy, policyPath);
            if (!confirm)
                return Content("Error in confirmation");

            if (ReportRunner.IndividualReportConfirmedByAll(policy))
            {
                ReportRunner.SetPolicyFullDraft(policy, policyPath);

                if (ReportRunner.TryRunReport(id) == true)
                    return Content("Report confirmed. Full draft sent");
                else
                    return Content("Report confirmed. Error in sending full draft report");
            }

            return Content("Report confirmed");
        }

       // Testing purpose
        [HttpGet]
        public ActionResult ResendIndividualDraft(string id, DateTime date, string draftKey)
        {
            if (date.Date != DateTime.Today)
                return Content("Cannot resend report for another date");

            var policyPath = ReportRunner.GetPolicyPath(id);
            var policy = Policy.CreateFromFile(policyPath);

            if (!ReportRunner.CheckConfirmed(draftKey, policy))
            {
                CmdProcess.RunProcess(policyPath, draftKey);
                return Content("Report resend");
            }

            return Content("Draft is already confirmed. Can't resend");                        
        }
    }
}