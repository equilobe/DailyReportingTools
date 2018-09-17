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
        public DashboardPage Get([FromUri] DashboardFilter filter)
        {
            if (!ReportService.IsDashboardAvailable(filter))
            {
                return new DashboardPage
                {
                    IsAvailable = false
                };
            }
            
            return ReportService.GetDashboardData(filter.InstanceId);
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
