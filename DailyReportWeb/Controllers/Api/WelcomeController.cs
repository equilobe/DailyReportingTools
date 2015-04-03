using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Web;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    public class WelcomeController : ApiController
    {
        public IDataService DataService { get; set; }

        public DataWelcome Get()
        {
            return new DataWelcome
            {
                ReportsGenerated = (DataService.GetNumberOfReportsGenerated() + 131400).ToString("###,###") + " reports generated"
            };
        }
    }
}
