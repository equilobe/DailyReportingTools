using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
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
        // GET: api/Policy
        public IEnumerable<PolicySummary> Get()
        {
            var baseUrl = User.GetBaseUrl();
            var sharedSecret = new DataService().GetSharedSecret(baseUrl);

            return new PolicySummaryService(baseUrl, sharedSecret).GetPoliciesSummary();
        }

        // GET: api/Policy/5
        public PolicyBuffer Get(long id)
        {
            var baseUrl = User.GetBaseUrl();
            var sharedSecret = new DataService().GetSharedSecret(baseUrl);

            return new PolicyService(baseUrl, sharedSecret).GetPolicy(id);
        }

        // PUT: api/Policy
        public void Post([FromBody]PolicySummary policySummary)
        {
            if(!Validations.Time(policySummary.ReportTime))
                throw new ArgumentException();

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

                    policySummary.CopyProperties(reportSettings);
                    reportSettings.InstalledInstanceId = installedInstance.Id;
                    reportSettings.UniqueProjectKey = ProjectService.GetUniqueProjectKey(policySummary.ProjectKey);

                    new TaskSchedulerService(reportSettings.UniqueProjectKey).Create(policySummary.ReportTime);
                }
                else
                {
                    if (reportSettings.ReportTime == policySummary.ReportTime)
                        return;

                    reportSettings.ReportTime = policySummary.ReportTime;
                    new TaskSchedulerService(reportSettings.UniqueProjectKey).Update(reportSettings.ReportTime);
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

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == updatedPolicy.BaseUrl);

                var reportSettings = installedInstance.ReportSettings.SingleOrDefault(qr => qr.ProjectId == updatedPolicy.ProjectId);
                if (reportSettings == null)
                {
                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);

                    updatedPolicy.CopyProperties<IReportSetting>(reportSettings);

                    reportSettings.InstalledInstanceId = installedInstance.Id;
                    reportSettings.UniqueProjectKey = ProjectService.GetUniqueProjectKey(updatedPolicy.ProjectKey);

                    if (!string.IsNullOrEmpty(reportSettings.ReportTime))
                        new TaskSchedulerService(reportSettings.UniqueProjectKey).Create(reportSettings.ReportTime);
                }
                else
                {
                    if (reportSettings.ReportTime != updatedPolicy.ReportTime)
                    {
                        reportSettings.ReportTime = updatedPolicy.ReportTime;
                        new TaskSchedulerService(reportSettings.UniqueProjectKey).Update(reportSettings.ReportTime);
                    }
                }

                var policy = new PolicyDetails();
                updatedPolicy.CopyProperties<ISerializedPolicy>(policy);

                if (reportSettings.SerializedPolicy == null)
                    reportSettings.SerializedPolicy = new SerializedPolicy();

                reportSettings.SerializedPolicy.PolicyString = Serialization.XmlSerialize(policy);

                db.SaveChanges();
            }
        }
    }
}
