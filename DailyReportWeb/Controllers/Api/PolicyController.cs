using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace DailyReportWeb.Controllers.Api
{
    [Authorize]
    public class PolicyController : ApiController
    {
        public IDataService DataService { get; set; }
        public IPolicyEditorService PolicyService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public PolicyBuffer Get(long id)
        {
            var baseUrl = User.GetBaseUrl();
            var username = User.GetUsername();

            var requestContext = new JiraRequestContext
            {
                BaseUrl = baseUrl,
                Username = username,
                SharedSecret = DataService.GetSharedSecret(baseUrl),
                Password = DataService.GetPassword(baseUrl, username)
            };

            return PolicyService.GetPolicy(id);
        }

        // PUT: api/Policy
        public void Post([FromBody]PolicySummary policySummary)
        {
            if(!Validations.Time(policySummary.ReportTime))
                throw new ArgumentException();


            var context = new ScheduledTaskContext
            {
                ReportTime = policySummary.ReportTime,
                UniqueProjectKey = policySummary.UniqueProjectKey
            };

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == policySummary.BaseUrl);

                var reportSettings = installedInstance.ReportSettings.SingleOrDefault(qr => qr.ProjectId == policySummary.ProjectId);
                if (reportSettings == null)
                {
                    if (string.IsNullOrEmpty(policySummary.ReportTime))
                        return;

                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);

                    policySummary.CopyPropertiesOnObjects(reportSettings);
                    reportSettings.InstalledInstanceId = installedInstance.Id;
                    reportSettings.UniqueProjectKey = RandomString.Get(policySummary.ProjectKey);

                    

                    TaskSchedulerService.CreateTask(context);
                }
                else
                {
                    if (reportSettings.ReportTime == policySummary.ReportTime)
                        return;

                    reportSettings.ReportTime = policySummary.ReportTime;

                    TaskSchedulerService.UpdateTask(context);
                }

                db.SaveChanges();
            }
        }

        // PUT: api/Policy/5
        public void Post(long id, [FromBody]PolicyBuffer updatedPolicy)
        {
            if (!Validations.Time(updatedPolicy.ReportTime) ||
                !Validations.Mails(updatedPolicy.DraftEmails) ||
                !Validations.Mails(updatedPolicy.Emails) ||
                (updatedPolicy.SourceControlOptions.Type == SourceControlType.SVN && !Validations.Url(updatedPolicy.SourceControlOptions.Repo)))
                throw new ArgumentException();


            var context = new ScheduledTaskContext
            {
                ReportTime = updatedPolicy.ReportTime,
                UniqueProjectKey = updatedPolicy.UniqueProjectKey
            };

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == updatedPolicy.BaseUrl);

                var reportSettings = installedInstance.ReportSettings.SingleOrDefault(qr => qr.ProjectId == updatedPolicy.ProjectId);
                if (reportSettings == null)
                {
                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);

                    updatedPolicy.CopyTo<IReportSetting>(reportSettings);

                    reportSettings.InstalledInstanceId = installedInstance.Id;
                    reportSettings.UniqueProjectKey = RandomString.Get(updatedPolicy.ProjectKey);

                    if (!string.IsNullOrEmpty(reportSettings.ReportTime))
                        TaskSchedulerService.CreateTask(context);
                }
                else
                {
                    if (reportSettings.ReportTime != updatedPolicy.ReportTime)
                    {
                        reportSettings.ReportTime = updatedPolicy.ReportTime;
                        TaskSchedulerService.UpdateTask(context);
                    }
                }

                var policy = new PolicyDetails();
                updatedPolicy.CopyTo<ISerializedPolicy>(policy);

                if (reportSettings.SerializedPolicy == null)
                    reportSettings.SerializedPolicy = new SerializedPolicy();

                reportSettings.SerializedPolicy.PolicyString = Serialization.XmlSerialize(policy);

                db.SaveChanges();
            }
        }
    }
}
