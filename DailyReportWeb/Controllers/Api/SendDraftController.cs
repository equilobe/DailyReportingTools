using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
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
    public class SendDraftController : ApiController
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
            var jiraUsers = JiraService.GetUsers(jiraRequestContext, jiraProject.Key);
            var sendResult = ReportExecutionService.SendDraft(context);
            var sendDetails = GetDraftConfirmationDetails(context, advancedSettings, jiraUsers, sendResult.HasError);

            return new DataReportOperation
            {
                Project = jiraProject.Name,
                Status = sendResult,
                Details = sendDetails
            };
        }

        private string GetDraftConfirmationDetails(ExecutionContext context, AdvancedReportSettings advancedSettings, List<JiraUser> jiraUsers, bool sendHasError)
        {
            if (DateTime.Compare(context.Date, DateTime.Today.Date) != 0)
                return "You are trying to resend a report sent another day, but you can only resend reports that were sent today.";

            var recipients = ReportExecutionService.GetFullDraftRecipients(advancedSettings);

            using (var db = new ReportsDb())
            {
                var basicSettingsId = db.BasicSettings.Single(bs => bs.UniqueProjectKey == context.Id).Id;
                var reportSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == basicSettingsId).LastFinalReportSentDate;

                if (reportSentDate.Value.Date == DateTime.Today)
                    return string.Format("The final report was already sent at {0} to {1}", reportSentDate.Value.ToShortTimeString(), recipients);
            }

            if (!ReportExecutionService.CanSendFullDraft(context) && !ReportExecutionService.IsForcedByLead(context))
            {
                var usersToConfirm = ReportExecutionService.GetRemainingUsersToConfirmIndividualDraft(context, jiraUsers);
                return string.Format("{0} must confirm. After everyone confirms, the full draft will be sent to {1}", usersToConfirm, recipients);
            }

            if (!sendHasError)
                return string.Format("The full draft will be resent shortly to {0}", recipients);

            return null;
        }
    }
}
