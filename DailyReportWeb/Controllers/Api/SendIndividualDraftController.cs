using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class SendIndividualDraftController : ApiController
    {
        public IReportExecutionService ReportExecutionService { get; set; }
        public IJiraService JiraService { get; set; }

        public DataReportOperation Post(ExecutionContext context)
        {
            long projectId;
            var jiraRequestContext = new JiraRequestContext();

            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Single(bs => bs.UniqueProjectKey == context.Id);
                basicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);

                projectId = basicSettings.ProjectId;
            }

            var jiraProject = JiraService.GetProject(jiraRequestContext, projectId);
            var confirmationResult = ReportExecutionService.SendIndividualDraft(context);

            return new DataReportOperation
            {
                Project = jiraProject.Name,
                Status = confirmationResult
            };
        }
    }
}
