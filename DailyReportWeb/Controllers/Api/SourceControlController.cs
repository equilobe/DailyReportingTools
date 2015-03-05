using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.SL;
using System.Collections.Generic;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class SourceControlController : ApiController
    {
        public List<string> Post([FromBody]SourceControlOptions sourceControlOptions)
        {
            return SourceControlService.GetContributors(sourceControlOptions);
        }
    }
}
