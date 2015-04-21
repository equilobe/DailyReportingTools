using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class ProjectsController : ApiController
    {
        public ISettingsService SettingsService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public List<BasicReportSettings> Get(long id)
        {
            return SettingsService.GetAllBasicReportSettings(new ItemContext(id));
        }

        public List<List<BasicReportSettings>> Get()
        {
            return SettingsService.GetAllBasicReportSettings(new UserContext());
        }
    }
}
