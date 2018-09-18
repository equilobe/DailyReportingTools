using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [AllowAnonymous]
    public class ReportController : BaseApiController
    {
        public IReportService ReportService { get; set; }

        [HttpGet]
        public DashboardData Get([FromUri] DashboardFilter filter)
        {
            if (!ReportService.IsDashboardAvailable(filter))
                return DashboardData.Unavailable();

            return ReportService.GetDashboardData(filter.InstanceId);
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
