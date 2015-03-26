using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Web;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class InstancesController : ApiController
    {
        public IDataService DataService { get; set; }

        public List<Instance> Get()
        {
            return DataService.GetInstances();
        }
    }
}
