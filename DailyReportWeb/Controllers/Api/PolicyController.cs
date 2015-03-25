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
    public class PolicyController : ApiController
    {
        public IDataService DataService { get; set; }
        public ISettingsService SettingsService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public FullReportSettings Get(long id)
        {
            return SettingsService.GetFullSettings(new ItemContext(id));
        }

        // PUT: api/Policy
        public void Post([FromBody]BasicReportSettings updatedBasicSettings)
        {
            if(!Validations.Time(updatedBasicSettings.ReportTime))
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

        // PUT: api/Policy/5
        public void Post(long id, [FromBody]FullReportSettings updatedFullSettings)
        {
            if (!Validations.Time(updatedFullSettings.ReportTime) ||
                !Validations.Mails(updatedFullSettings.DraftEmails) ||
                !Validations.Mails(updatedFullSettings.Emails) ||
                (updatedFullSettings.SourceControlOptions.Type == SourceControlType.SVN && !Validations.Url(updatedFullSettings.SourceControlOptions.Repo)))
                throw new ArgumentException();


            var context = new ScheduledTaskContext
            {
                ReportTime = updatedFullSettings.ReportTime,
                UniqueProjectKey = updatedFullSettings.UniqueProjectKey
            };

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == updatedFullSettings.BaseUrl);

                var basicSettings = installedInstance.BasicSettings.SingleOrDefault(qr => qr.ProjectId == updatedFullSettings.ProjectId);
                if (basicSettings == null)
                {
                    basicSettings = new BasicSettings();
                    db.BasicSettings.Add(basicSettings);

                    updatedFullSettings.CopyTo<IBasicSettings>(basicSettings);

                    basicSettings.InstalledInstanceId = installedInstance.Id;
                    basicSettings.UniqueProjectKey = RandomString.Get(updatedFullSettings.ProjectKey);

                    if (!string.IsNullOrEmpty(basicSettings.ReportTime))
                        TaskSchedulerService.CreateTask(context);
                }
                else
                {
                    if (basicSettings.ReportTime != updatedFullSettings.ReportTime)
                    {
                        basicSettings.ReportTime = updatedFullSettings.ReportTime;
                        TaskSchedulerService.UpdateTask(context);
                    }
                }

                var policy = new AdvancedReportSettings();
                updatedFullSettings.CopyTo<IAdvancedSettings>(policy);

                if (basicSettings.SerializedAdvancedSettings == null)
                    basicSettings.SerializedAdvancedSettings = new SerializedAdvancedSettings();

                basicSettings.SerializedAdvancedSettings.PolicyString = Serialization.XmlSerialize(policy);

                db.SaveChanges();
            }
        }
    }
}
