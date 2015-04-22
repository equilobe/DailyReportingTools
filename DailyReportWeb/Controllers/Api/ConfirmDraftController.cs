using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class ConfirmDraftController : ApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }

        public DataReportOperation Post(ExecutionContext context)
        {
            long projectId;
            var jiraRequestContext = new JiraRequestContext();
            var advancedSettings = new AdvancedReportSettings();

            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Single(bs => bs.UniqueProjectKey == context.Id);
                basicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);

                Deserialization.XmlDeserialize<AdvancedReportSettings>(basicSettings.SerializedAdvancedSettings.PolicyString)
                    .CopyPropertiesOnObjects(advancedSettings);

                projectId = basicSettings.ProjectId;
            }

            var jiraProject = JiraService.GetProject(jiraRequestContext, projectId);
            var confirmationResult = ReportExecutionService.SendReport(context);
            var confirmationDetails = GetDraftConfirmationDetails(context, advancedSettings, confirmationResult.HasError);

            return new DataReportOperation
            {
                Project = jiraProject.Name,
                Status = confirmationResult,
                Details = confirmationDetails
            };
        }

        private string GetDraftConfirmationDetails(ExecutionContext context, AdvancedReportSettings advancedSettings, bool confirmationHasError)
        {
            if (DateTime.Compare(context.Date, DateTime.Today.Date) != 0)
                return "You are trying to confirm a report sent another day, but you can only confirm reports that were sent today.";

            var recipients = ReportExecutionService.GetFinalReportRecipients(advancedSettings);

            using (var db = new ReportsDb())
            {
                var individualConfirmationBasicSettingsId = db.IndividualDraftConfirmations.Single(idc => idc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                var draftSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == individualConfirmationBasicSettingsId).LastDraftSentDate;

                if (draftSentDate.Value.Date == DateTime.Today)
                    return string.Format("The full draft report was already sent at {0} to {1}", draftSentDate.Value.ToShortTimeString(), recipients);
            }

            if (!confirmationHasError)
                string.Format("The final report will be sent shortly to {0}", recipients);

            return null;
        }
    }
}
