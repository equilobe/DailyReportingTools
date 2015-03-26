using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
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
            return SettingsService.GetAllBasicSettings(new ItemContext(id));
        }

        public void Post([FromBody]BasicReportSettings updatedBasicSettings)
        {
            if (!Validations.Time(updatedBasicSettings.ReportTime))
                throw new ArgumentException();

            var context = new ScheduledTaskContext
            {
                ReportTime = updatedBasicSettings.ReportTime,
                UniqueProjectKey = updatedBasicSettings.UniqueProjectKey
            };

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == updatedBasicSettings.BaseUrl);

                var basicSettings = installedInstance.BasicSettings.SingleOrDefault(qr => qr.ProjectId == updatedBasicSettings.ProjectId);
                if (basicSettings == null)
                {
                    if (string.IsNullOrEmpty(updatedBasicSettings.ReportTime))
                        return;

                    basicSettings = new BasicSettings();
                    db.BasicSettings.Add(basicSettings);

                    updatedBasicSettings.CopyPropertiesOnObjects(basicSettings);
                    basicSettings.InstalledInstanceId = installedInstance.Id;
                    basicSettings.UniqueProjectKey = RandomString.Get(updatedBasicSettings.ProjectKey);

                    TaskSchedulerService.CreateTask(context);
                }
                else
                {
                    if (basicSettings.ReportTime == updatedBasicSettings.ReportTime)
                        return;

                    basicSettings.ReportTime = updatedBasicSettings.ReportTime;

                    TaskSchedulerService.UpdateTask(context);
                }

                db.SaveChanges();
            }
        }
    }
}
