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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class ConfirmIndividualDraftController : ApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }

        public DataReportOperation Post(ExecutionContext context)
        {
            string username;
            long projectId;
            var jiraRequestContext = new JiraRequestContext();
            var advancedSettings = new AdvancedReportSettings();

            using (var db = new ReportsDb())
            {
                var individualConfirmation = db.IndividualDraftConfirmations.Single(idc => idc.UniqueUserKey == context.DraftKey);
                individualConfirmation.BasicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);

                Deserialization.XmlDeserialize<AdvancedReportSettings>(individualConfirmation.BasicSettings.SerializedAdvancedSettings.PolicyString)
                    .CopyPropertiesOnObjects(advancedSettings);

                username = individualConfirmation.Username;
                projectId = individualConfirmation.BasicSettings.ProjectId;
            }

            var jiraProject = JiraService.GetProject(jiraRequestContext, projectId);
            var jiraUsers = JiraService.GetUsers(jiraRequestContext, jiraProject.Key);
            var jiraDisplayName = JiraService.GetUser(jiraRequestContext, username).DisplayName;

            var confirmationResult = ReportExecutionService.ConfirmIndividualDraft(context);
            var confirmationDetails = GetIndividualDraftConfirmationDetails(context, advancedSettings, jiraUsers, confirmationResult.HasError);

            return new DataReportOperation
            {
                User = jiraDisplayName,
                Project = jiraProject.Name,
                Status = confirmationResult,
                Details = confirmationDetails
            };
        }

        private string GetIndividualDraftConfirmationDetails(ExecutionContext context, AdvancedReportSettings advancedSettings, List<JiraUser> jiraUsers, bool confirmationHasError)
        {
            var offsetFromUtc = DataService.GetOffsetFromProjectKey(context.Id);

            if (DateTimeHelpers.CompareDay(context.Date, DateTime.Now.ToOriginalTimeZone(offsetFromUtc)) != 1)
                return "You are trying to confirm a report sent another day, but you can only confirm reports that were sent today.";

            var recipients = ReportExecutionService.GetFullDraftRecipients(advancedSettings);

            using (var db = new ReportsDb())
            {
                var individualConfirmationBasicSettingsId = db.IndividualDraftConfirmations.Single(idc => idc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                var draftSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == individualConfirmationBasicSettingsId).LastDraftSentDate;

                if (DateTimeHelpers.CompareDay(draftSentDate, DateTime.Now, offsetFromUtc) == 1)
                    return string.Format("The full draft report was already sent at {0} to {1}", draftSentDate.Value.ToShortTimeString(), recipients);
            }

            var confirmationContext = new ConfirmationContext
            {
                ExecutionContext = context,
                Users = jiraUsers,
                OffsetFromUtc = offsetFromUtc
            };

            if (ReportExecutionService.CanSendFullDraft(confirmationContext).HasError)
            {
                var usersToConfirm = ReportExecutionService.GetRemainingUsersToConfirmIndividualDraft(confirmationContext);
                return string.Format("{0} must confirm. After everyone confirms, the full draft will be sent to {1}", usersToConfirm, recipients);
            }

            if (!confirmationHasError)
                return string.Format("You were the last one to confirm. The full draft will be sent shortly to {0}", recipients);

            return null;
        }
    }
}
