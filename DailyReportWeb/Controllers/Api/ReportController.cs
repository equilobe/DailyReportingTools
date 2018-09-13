using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using System.Collections.Generic;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [AllowAnonymous]
    public class ReportController : BaseApiController
    {
        public IReportService ReportService { get; set; }

        [HttpGet]
        public List<DashboardItem> Get(long id)
        {
            return ReportService.GetDashboardData(id);
        }

        [HttpGet]
        public bool IsDashboardAvailable([FromUri] DashboardFilter filter)
        {
            return ReportService.IsDashboardAvailable(filter);
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
