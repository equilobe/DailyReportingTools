﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
            var sharedSecret = DbService.GetSharedSecret(baseUrl);

            return PolicySummaryService.GetPoliciesSummary(baseUrl, sharedSecret);
        }

        // GET: api/Policy/5
        public PolicyBuffer Get(long id)
        {
            var baseUrl = User.GetBaseUrl();
            var sharedSecret = DbService.GetSharedSecret(baseUrl);

            return PolicyService.GetPolicy(baseUrl, sharedSecret, id);
        }

        // PUT: api/Policy
        public void Put([FromBody]PolicySummary policySummary)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == policySummary.BaseUrl);

                var reportSettings = installedInstance.ReportSettings.SingleOrDefault(qr => qr.ProjectId == policySummary.ProjectId);
                if (reportSettings == null)
                {
                    if (policySummary.ReportTime == null)
                        return;

                    reportSettings = new ReportSettings();
                    db.ReportSettings.Add(reportSettings);

                    policySummary.CopyProperties(reportSettings);

                    reportSettings.InstalledInstanceId = installedInstance.Id;
                    reportSettings.UniqueProjectKey = ProjectService.GetUniqueProjectKey(policySummary.ProjectKey);
                }
                else
                {
                    if (reportSettings.ReportTime == policySummary.ReportTime)
                        return;

                    reportSettings.ReportTime = policySummary.ReportTime;
                }

                db.SaveChanges();
            }
        }

        // PUT: api/Policy/5
        public void Put(long id, [FromBody]PolicyBuffer updatedPolicy)
        {
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
                }
                else
                {
                    if (reportSettings.ReportTime != updatedPolicy.ReportTime)
                        reportSettings.ReportTime = updatedPolicy.ReportTime;
                }

                var policy = new PolicyDetails();
                updatedPolicy.CopyProperties<ISerializedPolicy>(policy);
                if (reportSettings.SerializedPolicy == null)
                    reportSettings.SerializedPolicy = new SerializedPolicy();
                reportSettings.SerializedPolicy.PolicyString = Serialization.XmlSerialize(policy);

                db.SaveChanges();
            }
        }

        // Policies are not created directly!
        // POST: api/Policy
        //public void Post([FromBody]string value)
        //{
        //}

        // Policies are not deleted directly!
        // DELETE: api/Policy/5
        //public void Delete(int id)
        //{
        //}
    }
}
