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
using System.Data.Entity;
using System.Net.Http;


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

            SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));
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
                SettingsService.SyncAllBasicSettings(new ItemContext(instanceId));
        }

        public void DeleteInstance(string pluginKey)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.Single(qr => qr.ClientKey == pluginKey);
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public void DeleteInstance(long id)
        {
            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                var installedInstance = db.InstalledInstances.Where(ii => ii.UserId == userId && ii.Id == id).Single();
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public void ActivateInstance(SubscriptionContext context)
        {
            using(var db = new ReportsDb())
            {
                var instance = db.InstalledInstances.Single(i => i.BaseUrl == context.BaseUrl && i.User.UserName == context.Username);
                instance.Active = true;

                var subscription = new SubscriptionDetails();
                subscription.TrialStartDate = context.TrialStartDate;
                subscription.TrialEndDate = context.TrialEndDate;
                subscription.SubscriptionDate = context.SubscriptionDate;
                subscription.SubscriptionId = context.SubscriptionId;
                subscription.PaymentGross = context.PaymentGross;
                subscription.InstalledInstanceId = instance.Id;
                instance.SubscriptionDetails = subscription;
                

                db.SaveChanges();
            }
        }

        public string GetSharedSecret(string baseUrl)
        {
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                    .Where(x => x.BaseUrl == baseUrl)
                    .Select(x => x.SharedSecret)
                    .FirstOrDefault();
            }
        }

        public string GetPassword(string baseUrl, string username)
        {
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                    .Where(ii => ii.BaseUrl == baseUrl && ii.JiraUsername == username)
                    .Select(ii => ii.JiraPassword)
                    .FirstOrDefault();
            }
        }

        public string GetReportTime(string baseUrl, long projectId)
        {
            using (var db = new ReportsDb())
            {
                return db.BasicSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
            }
        }

        public List<string> GetUniqueProjectsKey(string pluginKey)
        {
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                         .Where(installedInstance => installedInstance.ClientKey == pluginKey)
                         .SelectMany(installedInstance => installedInstance.BasicSettings.Select(reportSettings => reportSettings.UniqueProjectKey))
                .ToList();
        }
        }

        public List<string> GetUniqueProjectsKey(long id)
        {
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                         .Where(installedInstance => installedInstance.Id == id)
                         .SelectMany(installedInstance => installedInstance.BasicSettings.Select(reportSettings => reportSettings.UniqueProjectKey))
                         .ToList();
            }
        }

        public List<Instance> GetInstances()
        {
            var userId = new UserContext().UserId;
            var instances = new List<Instance>();

            using (var db = new ReportsDb())
            {
                db.InstalledInstances
                           .Where(ii => ii.UserId == userId)
                           .ToList()
                           .ForEach(installedInstance =>
                {
                               instances.Add(new Instance
                               {
                                   Id = installedInstance.Id,
                                   BaseUrl = installedInstance.BaseUrl,
                                   TimeZone = installedInstance.TimeZone
                               });
                           });
            }

            return instances;
        }

        public long GetNumberOfReportsGenerated()
        {
            using (var db = new ReportsDb())
            {
                return db.ReportExecutionInstances.ToList().Last().Id;
            }
        }

        public string GetBaseUrl(long id)
        {
            using (var db = new ReportsDb())
            {
                return db.InstalledInstances
                .Where(installedInstance => installedInstance.Id == id)
                .Select(installedInstance => installedInstance.BaseUrl)
                .FirstOrDefault();
            }
        }

        public bool IsInstanceActive(long basicSettingsId)
        {
            using(var db = new ReportsDb())
            {
                var instance = SearchInstanceBySettingsId(basicSettingsId);

                return instance.Active.Value;
            }
        }         


        // TODO: refactor to return a different object that is not tied to the Storage layer
        public BasicSettings GetReportSettingsWithDetails(string uniqueProjectKey)
        {
            using (var db = new ReportsDb())
            {
                return db.BasicSettings
                    .Include(s => s.InstalledInstance.User)
                    .Include(s => s.ReportExecutionSummary)
                    .Include(s => s.FinalDraftConfirmation)
                    .Include(s => s.IndividualDraftConfirmations)
                    .Include(s => s.ReportExecutionInstances)
                    .SingleOrDefault(r => r.UniqueProjectKey == uniqueProjectKey);
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

        public TimeSpan GetOffsetFromProjectKey(string key)
        {
            var timeZoneId = GetTimeZoneIdFromProjectKey(key);
            return TimeZoneHelpers.GetOffsetFromTimezoneId(timeZoneId);
        }

        #region helpers
        string GetTimeZoneIdFromProjectKey(string uniqueProjcetKey)
        {
            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Single(bs => bs.UniqueProjectKey == uniqueProjcetKey);
                return basicSettings.InstalledInstance.TimeZone;
            }
        }

        InstalledInstance SearchInstanceBySettingsId(long settingsId)
        {
            using(var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.Single(bs => bs.Id == settingsId);

                return basicSettings.InstalledInstance;
            }
        }
        #endregion
    }
}
