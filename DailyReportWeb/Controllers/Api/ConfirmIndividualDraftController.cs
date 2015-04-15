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
            var projectKey = string.Empty;
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
            if (confirmationResult.HasError)
            {

            }
            else
            {
                if (ReportExecutionService.CanSendFullDraft(context))
                {
                    confirmationDetails.Append("You were the last one to confirm. The full draft will be sent shortly to");
                }
                else
                {
                    var usernamesToConfirm = new List<string>();
                    var usersToConfirm = new List<string>();

                    using (var db = new ReportsDb())
                    {
                        var basicSettingsId = db.IndividualDraftConfirmations.Single(uidc => uidc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                        usernamesToConfirm = db.IndividualDraftConfirmations.Where(idc => idc.ReportDate == context.Date &&
                                                                                          idc.LastDateConfirmed != DateTime.Today &&
                                                                                          idc.BasicSettingsId == basicSettingsId)
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

                    foreach (var userToConfirm in usersToConfirm)
                    {
                        if (usersToConfirm.Count > 1)
                        {
                            if (userToConfirm.Equals(usersToConfirm.Last()))
                                confirmationDetails.Append(" and ");
                            else
                                confirmationDetails.Append(", ");
                        }

                        confirmationDetails.AppendFormat("{0}", userToConfirm);
                    }

                    confirmationDetails.Append(" must confirm. The full draft will be sent after everyone confirms to");
                }

                if (advancedSettings.AdvancedOptions.SendDraftToProjectManager)
                    confirmationDetails.Append(" the project lead");

                if (advancedSettings.AdvancedOptions.SendFinalToOthers)
                {
                    var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                            .ToList();

                    foreach (var email in emails)
                    {
                        if (email.Equals(emails.Last()))
                            confirmationDetails.Append(" and");
                        else
                            confirmationDetails.Append(",");

                        confirmationDetails.AppendFormat(" {0}", email);
                    }
                }
            }

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
