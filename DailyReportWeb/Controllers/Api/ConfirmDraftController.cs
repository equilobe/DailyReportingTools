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
    public class ConfirmDraftController : ApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }

        public DataConfirmIndividualDraft Post(ExecutionContext context)
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
            var confirmationDetails = new StringBuilder();

            if (ReportExecutionService.CanSendFullDraft(context))
            {
                if (confirmationResult.HasError)
                {
                    DateTime? reportSentDate;
                    using (var db = new ReportsDb())
                    {
                        var basicSettingsId = db.BasicSettings.Single(bs => bs.UniqueProjectKey == context.Id).Id;
                        reportSentDate = db.ReportExecutionSummaries.Single(res => res.BasicSettingsId == basicSettingsId).LastFinalReportSentDate;
                    }

                    if (reportSentDate.Value.Date == DateTime.Today)
                        confirmationDetails.AppendFormat("The final report was already sent at {0}", reportSentDate.Value.ToShortTimeString());
                }
                else
                {
                    confirmationDetails.Append("The final report will be sent shortly to");
                }
            }

            var fullDraftRecipients = new List<string>();
            if (advancedSettings.AdvancedOptions.SendFinalToAllUsers)
                fullDraftRecipients.Add("the entire team");

            if (advancedSettings.AdvancedOptions.SendFinalToOthers)
            {
                var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();
                fullDraftRecipients.AddRange(emails);
            }

            confirmationDetails.AppendFormat(" {0}", StringExtensions.GetNaturalLanguage(fullDraftRecipients));

            return new DataConfirmIndividualDraft
            {
                Project = jiraProject.Name,
                Status = confirmationResult,
                Details = confirmationDetails.ToString()
            };
        }
    }
}
