using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class DashboardSyncController : BaseApiController
    {
        public IReportService ReportService { get; set; }

        [HttpPost]
        public SimpleResult Post([FromUri] string instanceUniqueKey)
        {
            return ReportService.SyncDashboardData(instanceUniqueKey);
        }
    }
}
