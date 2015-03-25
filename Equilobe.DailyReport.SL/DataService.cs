using Encryptamajig;
using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Equilobe.DailyReport.SL
{
    public class DataService : IDataService
    {
        public void SaveInstance(InstalledInstance instanceData)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(i => i.BaseUrl == instanceData.BaseUrl && i.ClientKey == instanceData.ClientKey);
                if (installedInstance == null)
                    db.InstalledInstances.Add(instanceData);
                else
                    installedInstance.SharedSecret = instanceData.SharedSecret;

                db.SaveChanges();
            }
        }

        public void SaveInstance(RegisterModel modelData)
        {
            using (var db = new ReportsDb())
            {
                var instanceData = new InstalledInstance();
                modelData.JiraPassword = AesEncryptamajig.Encrypt(modelData.JiraPassword, GetEncriptedKey());
                modelData.CopyPropertiesOnObjects(instanceData);
                instanceData.UserId = db.Users.Where(u => u.Email == modelData.Email)
                                              .Select(u => u.Id)
                                              .Single();

                db.InstalledInstances.Add(instanceData);

                db.SaveChanges();
            }
        }

        public void DeleteInstance(string baseUrl)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == baseUrl);
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public string GetSharedSecret(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(x => x.BaseUrl == baseUrl)
                .Select(x => x.SharedSecret)
                .FirstOrDefault();
        }

        public string GetPassword(string baseUrl, string username)
        {
            var encryptedPass = new ReportsDb().InstalledInstances
                .Where(ii => ii.BaseUrl == baseUrl && ii.JiraUsername == username)
                .Select(ii => ii.JiraPassword)
                .FirstOrDefault();

            return AesEncryptamajig.Decrypt(encryptedPass, GetEncriptedKey());
        }

        public string GetEncriptedKey()
        {
            var byteKey = Convert.FromBase64String(ConfigurationManager.AppSettings["encKey"]);
            var encKey = Encoding.UTF8.GetString(byteKey);
            return encKey;
        }

        public string GetReportTime(string baseUrl, long projectId)
        {
            return new ReportsDb().BasicSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
        }

        public List<string> GetUniqueProjectsKey(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.BaseUrl == baseUrl)
                .SelectMany(installedInstance => installedInstance.BasicSettings
                .Select(reportSettings => reportSettings.UniqueProjectKey))
                .ToList();
        }

        public List<Instance> GetInstances(ApplicationUser user)
        {
            var installedInstances = user.InstalledInstances.ToList();
            var instances = new List<Instance>();

            installedInstances.ForEach(installedInstance =>
                {
                    instances.Add(new Instance { 
                        Id = installedInstance.Id,
                        BaseUrl = installedInstance.BaseUrl
                    });
                });

            return instances;
        }

        public string GetBaseUrl(long id)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.Id == id)
                .Select(installedInstance => installedInstance.BaseUrl)
                .FirstOrDefault();
        }

        public void SetReportFromDb(JiraReport _report)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == _report.UniqueProjectKey);
                _report.Settings = new BasicSettings();
                reportSettings.CopyTo<BasicSettings>(_report.Settings);
                if (reportSettings.ReportExecutionSummary != null)
                {
                    if (reportSettings.ReportExecutionSummary.LastDraftSentDate != null)
                        _report.LastDraftSentDate = reportSettings.ReportExecutionSummary.LastDraftSentDate.Value;
                    if (reportSettings.ReportExecutionSummary.LastFinalReportSentDate != null)
                        _report.LastReportSentDate = reportSettings.ReportExecutionSummary.LastFinalReportSentDate.Value;
                }

                if (reportSettings.FinalDraftConfirmation != null)
                    if (reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate != null)
                        _report.LastFinalDraftConfirmationDate = reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate.Value;
            }
        }

        FullReportSettings GetPolicyBufferFromDb(string uniqueProjectKey)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == uniqueProjectKey);

                if (reportSettings == null || reportSettings.SerializedAdvancedSettings == null)
                    return null;

                var policyBuffer = new FullReportSettings();
                reportSettings.CopyTo<IBasicSettings>(policyBuffer);

                if (!string.IsNullOrEmpty(reportSettings.SerializedAdvancedSettings.PolicyString))
                    Deserialization.XmlDeserialize<ReportPolicy>(reportSettings.SerializedAdvancedSettings.PolicyString)
                        .CopyPropertiesOnObjects(policyBuffer);

                return policyBuffer;
            }
        }

        public JiraPolicy GetPolicy(string uniqueProjectKey)
        {
            var policyBuffer = GetPolicyBufferFromDb(uniqueProjectKey);
            var policy = new JiraPolicy();
            policyBuffer.CopyPropertiesOnObjects(policy);
            return policy;
        }
    }
}
