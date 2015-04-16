using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class ConfirmIndividualDraftController : ApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }

        public DataConfirmIndividualDraft Post(ExecutionContext context)
        {
            var username = string.Empty;
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
            var jiraDisplayName = JiraService.GetUser(jiraRequestContext, username).displayName;

            var confirmationResult = ReportExecutionService.ConfirmIndividualDraft(context);
            var confirmationDetails = new StringBuilder();

            if (ReportExecutionService.CanSendFullDraft(context))
            {
                if (confirmationResult.HasError)
                {
                    DateTime? draftSentDate;
                    using (var db = new ReportsDb())
                    {
                        var individualConfirmationBasicSettingsId = db.IndividualDraftConfirmations.Single(idc => idc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                        draftSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == individualConfirmationBasicSettingsId).LastDraftSentDate;
                    }

                    if (draftSentDate.Value.Date == DateTime.Today)
                        confirmationDetails.AppendFormat("The full draft report was already sent at {0}", draftSentDate.Value.ToShortTimeString());
                    else
                        confirmationDetails.Append("The full draft report will be sent shortly to");
                }
                else
                {
                    confirmationDetails.Append("You were the last one to confirm. The full draft will be sent shortly to");
                }
            }
            else
            {
                var usernamesToConfirm = new List<string>();
                var usersToConfirm = new List<string>();

                using (var db = new ReportsDb())
                {
                    var basicSettingsId = db.IndividualDraftConfirmations.Single(uidc => uidc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                    usernamesToConfirm = db.IndividualDraftConfirmations.Where(idc => idc.BasicSettingsId == basicSettingsId &&
                                                                                      idc.ReportDate == context.Date &&
                                                                                      DbFunctions.TruncateTime(idc.LastDateConfirmed) != DateTime.Today)
                                                                        .Select(qr => qr.Username)
                                                                        .ToList();
                }

                usernamesToConfirm.ForEach(usernameToConfirm =>
                {
                    jiraUsers.ForEach(jiraUser =>
                    {
                        if (usernameToConfirm == jiraUser.name)
                            usersToConfirm.Add(jiraUser.displayName);
                    });
                });

                confirmationDetails.AppendFormat("{0}", StringExtensions.GetNaturalLanguage(usersToConfirm));

                confirmationDetails.Append(" must confirm. After everyone confirms, the full draft will be sent to");
            }

            var fullDraftRecipients = new List<string>();
            if (advancedSettings.AdvancedOptions.SendDraftToProjectManager)
                fullDraftRecipients.Add("the project lead");

            if (advancedSettings.AdvancedOptions.SendDraftToOthers)
            {
                var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();
                fullDraftRecipients.AddRange(emails);
            }

            confirmationDetails.AppendFormat(" {0}", StringExtensions.GetNaturalLanguage(fullDraftRecipients));

            return new DataConfirmIndividualDraft
            {
                User = jiraDisplayName,
                Project = jiraProject.Name,
                Status = confirmationResult,
                Details = confirmationDetails.ToString()
            };
        }
    }
}
