using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ReportController : ApiController
    {
        public IReportService ReportService { get; set; }

        [HttpGet]
        public object Get(long id)
        {
            return null;
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }

        [HttpPost]
        public SimpleResult SyncJiraDB(string instanceUniqueKey)
        {
            return ReportService.TrySyncJiraDB(instanceUniqueKey);
        }
    }
}
