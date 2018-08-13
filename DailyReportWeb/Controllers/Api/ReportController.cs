using Equilobe.DailyReport.Models.Interfaces;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ReportController : ApiController
    {
        public IReportService ReportService { get; set; }

        public object Get(long id)
        {
            return null;
        }

        public void Post(long id)
        {
            ReportService.UpdateDashboardData(id);
        }
    }
}
