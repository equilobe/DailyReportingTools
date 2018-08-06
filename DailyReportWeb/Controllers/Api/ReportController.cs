using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ReportController : ApiController
    {
        public object Get(long id)
        {
            return null;
        }
    }
}
