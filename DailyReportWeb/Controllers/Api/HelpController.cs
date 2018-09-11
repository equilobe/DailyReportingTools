using Equilobe.DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class HelpController : BaseApiController
    {
        public SimpleResult Get()
        {
            return SimpleResult.Success("");
        }
    }
}
