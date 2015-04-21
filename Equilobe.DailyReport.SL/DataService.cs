using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class DataService : IDataService
    {
        public IEncryptionService EncryptionService { get; set; }
        public ISettingsService SettingsService { get; set; }

        public void SaveInstance(InstalledInstance instanceData)
        {
            long instanceId;
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(i => i.BaseUrl == instanceData.BaseUrl && i.ClientKey == instanceData.ClientKey);
                if (installedInstance == null)
                    db.InstalledInstances.Add(instanceData);
                else
                    installedInstance.SharedSecret = instanceData.SharedSecret;

                db.SaveChanges();

                instanceId = installedInstance.Id;
            }

            SettingsService.SetAllBasicSettings(new ItemContext(instanceId));
        }

        public void SaveInstance(RegisterModel modelData)
        {
            long instanceId = 0;
            using (var db = new ReportsDb())
            {
                var user = db.Users.Where(u => u.Email == modelData.Email)
                                              .Single();

                var installedInstance = user.InstalledInstances.SingleOrDefault(i => i.BaseUrl == modelData.BaseUrl);
                if (installedInstance != null && !string.IsNullOrEmpty(modelData.Password))
                    throw new ArgumentException();

                if (installedInstance == null)
                {
                    installedInstance = new InstalledInstance();
                    db.InstalledInstances.Add(installedInstance);
                    installedInstance.UserId = user.Id;
                }
                modelData.JiraPassword = EncryptionService.Encrypt(modelData.JiraPassword);
                modelData.CopyPropertiesOnObjects(installedInstance);

                db.SaveChanges();

                if (user.EmailConfirmed)
                    instanceId = installedInstance.Id;
            }

            if (instanceId != 0)
                SettingsService.SetAllBasicSettings(new ItemContext(instanceId));
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
            return new ReportsDb().InstalledInstances
                .Where(ii => ii.BaseUrl == baseUrl && ii.JiraUsername == username)
                .Select(ii => ii.JiraPassword)
                .FirstOrDefault();
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

        public List<Instance> GetInstances()
        {
            var userId = new UserContext().UserId;
            var instances = new List<Instance>();

            new ReportsDb().InstalledInstances
                           .Where(ii => ii.UserId == userId)
                           .ToList()
                           .ForEach(installedInstance =>
                {
                    var instance = new Instance();
                    instance.CopyFrom<IInstance>(installedInstance);

                    instances.Add(instance);
                });

            return instances;
        }

        public long GetNumberOfReportsGenerated()
        {
            return new ReportsDb().ReportExecutionInstances.ToList().Last().Id;
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
                reportSettings.InstalledInstance.CopyTo<InstalledInstance>(_report.Settings.InstalledInstance);
                reportSettings.InstalledInstance.UserImages.CopyTo<ICollection<UserImage>>(_report.Settings.InstalledInstance.UserImages);

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
                    Deserialization.XmlDeserialize<AdvancedReportSettings>(reportSettings.SerializedAdvancedSettings.PolicyString)
                        .CopyPropertiesOnObjects(policyBuffer);

                return policyBuffer;
            }
        }

        public byte[] GetUserImageByKey(string key)
        {
            using (var db = new Equilobe.DailyReport.DAL.ReportsDb())
            {
                return db.UserImages.Single(i => i.Key == key).ImageContent;
            }
        }

        public JiraPolicy GetPolicy(string uniqueProjectKey)
        {
            var policyBuffer = GetPolicyBufferFromDb(uniqueProjectKey);
            var policy = new JiraPolicy();
            policyBuffer.CopyPropertiesOnObjects(policy);
            return policy;
        }

        public void AddUserImage(UserImageContext context)
        {
            using (var db = new ReportsDb())
            {
                var avatar = db.UserImages.SingleOrDefault(userImage => userImage.Username == context.Username);
                if (avatar != null)
                    return;

                db.UserImages.Add(new UserImage
                {
                    ImageContent = context.Image,
                    Key = RandomString.Get(),
                    Username = context.Username,
                    InstalledInstanceId = context.InstanceId
                });

                db.SaveChanges();
            }
        }

        public string GetUserImageKey(string username)
        {
            using (var db = new ReportsDb())
            {
                var avatar = db.UserImages.Single(im => im.Username == username);

                return avatar.Key;
            }
        }
    }
}
