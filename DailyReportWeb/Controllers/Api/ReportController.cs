using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using System.Collections.Generic;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [AllowAnonymous]
    public class ReportController : ApiController
    {
        public IReportService ReportService { get; set; }

        [HttpGet]
        public List<DashboardItem> Get(long id)
        {
            return ReportService.GetDashboardData(id);
        }

        [HttpPost]
        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
