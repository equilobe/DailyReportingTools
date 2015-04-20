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

        private static string GetDraftConfirmationDetails(ExecutionContext context, AdvancedReportSettings advancedSettings, bool confirmationHasError)
        {
            if (DateTime.Compare(context.Date, DateTime.Today.Date) != 0)
                return "You are trying to confirm a report sent another day, but you can only confirm reports that were sent today.";

            var recipients = GetConfirmationRecipients(advancedSettings);

            if (!confirmationHasError)
                return string.Format("The final report will be sent shortly to {0}", recipients);

            DateTime? reportSentDate;
            using (var db = new ReportsDb())
            {
                var basicSettingsId = db.BasicSettings.Single(bs => bs.UniqueProjectKey == context.Id).Id;
                reportSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == basicSettingsId).LastFinalReportSentDate;
            }

            if (reportSentDate.Value.Date == DateTime.Today)
                return string.Format("The final report was already sent at {0} to {1}", reportSentDate.Value.ToShortTimeString(), recipients);

            return string.Format("The final report will be sent shortly to {0}", recipients);
        }

        private static string GetConfirmationRecipients(AdvancedReportSettings advancedSettings)
        {
            var fullDraftRecipients = new List<string>();

            if (advancedSettings.AdvancedOptions.SendFinalToAllUsers)
                fullDraftRecipients.Add("the entire team");
            else
                fullDraftRecipients.Add("the project lead");

            if (advancedSettings.AdvancedOptions.SendFinalToOthers)
            {
                var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();
                fullDraftRecipients.AddRange(emails);
            }

            return StringExtensions.GetNaturalLanguage(fullDraftRecipients);
        }
    }
}
