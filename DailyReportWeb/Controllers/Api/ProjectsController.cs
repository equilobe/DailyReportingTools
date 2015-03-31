using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
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

        public void Post([FromBody]BasicReportSettings updatedBasicSettings)
        {
            if (!Validations.Time(updatedBasicSettings.ReportTime))
                throw new ArgumentException();

            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Single(qr => qr.Id == updatedBasicSettings.Id);
                if (basicSettings.ReportTime == updatedBasicSettings.ReportTime)
                    return;

                basicSettings.ReportTime = updatedBasicSettings.ReportTime;
                TaskSchedulerService.SetTask(new ScheduledTaskContext
                {
                    ReportTime = updatedBasicSettings.ReportTime,
                    UniqueProjectKey = updatedBasicSettings.UniqueProjectKey
                });

                db.SaveChanges();
            }
        }
    }
}
