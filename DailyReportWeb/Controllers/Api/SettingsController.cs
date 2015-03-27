using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class SettingsController : ApiController
    {
        public ISettingsService SettingsService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public FullReportSettings Get(long id)
        {
            return SettingsService.GetSyncedReportSettings(new ItemContext(id));
        }

        public void Post(long id, [FromBody]FullReportSettings updatedFullSettings)
        {
            if (!Validations.Time(updatedFullSettings.ReportTime) ||
                !Validations.Mails(updatedFullSettings.DraftEmails) ||
                !Validations.Mails(updatedFullSettings.Emails) ||
                (updatedFullSettings.SourceControlOptions.Type == SourceControlType.SVN && !Validations.Url(updatedFullSettings.SourceControlOptions.Repo)))
                throw new ArgumentException();

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == updatedFullSettings.BaseUrl);
                var basicSettings = installedInstance.BasicSettings.SingleOrDefault(qr => qr.ProjectId == updatedFullSettings.ProjectId);

                if (basicSettings.ReportTime != updatedFullSettings.ReportTime)
                {
                    basicSettings.ReportTime = updatedFullSettings.ReportTime;
                    TaskSchedulerService.SetTask(new ScheduledTaskContext
                    {
                        ReportTime = updatedFullSettings.ReportTime,
                        UniqueProjectKey = updatedFullSettings.UniqueProjectKey
                    });
                }

                var advancedSettings = new AdvancedReportSettings();
                updatedFullSettings.CopyTo<IAdvancedSettings>(advancedSettings);

                if (basicSettings.SerializedAdvancedSettings == null)
                    basicSettings.SerializedAdvancedSettings = new SerializedAdvancedSettings();
                basicSettings.SerializedAdvancedSettings.PolicyString = Serialization.XmlSerialize(advancedSettings);

                db.SaveChanges();
            }
        }
    }
}
