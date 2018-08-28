using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ReportController : ApiController
    {
        public IReportService ReportService { get; set; }

        [HttpGet]
        public Page<DashboardItem> Get([FromUri] InstanceFilter filter)
        {
            return ReportService.GetDashboardData(filter);
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
