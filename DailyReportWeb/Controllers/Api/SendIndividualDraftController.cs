using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class SendIndividualDraftController : BaseApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }

        public DataReportOperation Post(ExecutionContext context)
        {
            string username;
            long projectId;
            var jiraRequestContext = new JiraRequestContext();

            using (var db = new ReportsDb())
            {
                var individualConfirmation = db.IndividualDraftConfirmations.Single(idc => idc.UniqueUserKey == context.DraftKey);
                individualConfirmation.BasicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);

                username = individualConfirmation.Username;
                projectId = individualConfirmation.BasicSettings.ProjectId;
            }

            var jiraDisplayName = JiraService.GetUser(jiraRequestContext, username).DisplayName;
            var jiraProject = JiraService.GetProject(jiraRequestContext, projectId);
            var confirmationResult = ReportExecutionService.SendIndividualDraft(context);

            return new DataReportOperation
            {
                User = jiraDisplayName,
                Project = jiraProject.Name,
                Status = confirmationResult
            };
        }
    }
}
