using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ProjectController : ApiController
    {
        public IDataService DataService { get; set; }

        public List<InstalledInstance> Get()
        {
            var username = User.GetUsername();
            return DataService.GetInstances(username);
        }

        public IEnumerable<PolicySummary> Get(long id)
        {
            var username = User.GetUsername();
            var baseUrl = DataService.GetBaseUrl(id);

            var requestContext = new JiraRequestContext
            {
                BaseUrl = baseUrl,
                Username = username,
                SharedSecret = DataService.GetSharedSecret(baseUrl),
                Password = DataService.GetPassword(baseUrl, username)
            };

            return new PolicySummaryService().GetPoliciesSummary();
        }
    }
}
