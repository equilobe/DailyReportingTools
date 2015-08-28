using Equilobe.DailyReport.Models.PayPal;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IDataService : IService
    {
        void SaveInstance(InstalledInstance instanceData);
        SimpleResult SaveInstance(RegisterModel modelData);
        void DeleteInstance(long id);
        void DeleteInstance(string pluginKey);
        void SetInstanceExpirationDate(string subscriptionId, DateTime date);
        void SetInstanceExpirationDate(long instanceId, DateTime date);
        Subscription GetSubscription(string subscriptionId);
        InstalledInstance GetInstance(string subscriptionId);
        ApplicationUser GetUser(string userId);
        //void ActivateInstance(string username, string baseUrl);
        //void ActivateInstance(string subscriptionId);
        void DeactivateInstance(string subscriptionId);
        bool IsInstanceActive(string subscriptionId);
        void SaveSubscription(SubscriptionContext context);
        void SavePayment(PaymentContext context);
        bool IsInstanceActive(long id);
        List<Instance> GetInstances();
        long GetNumberOfReportsGenerated();
        string GetBaseUrl(long id);
        string GetSharedSecret(string baseUrl);
        string GetPassword(string baseUrl, string username);
        string GetReportTime(string baseUrl, long projectId);
        List<string> GetUniqueProjectsKey(long id);
        List<string> GetUniqueProjectsKey(string pluginKey);
        BasicSettings GetReportSettingsWithDetails(string uniqueProjectKey);
        JiraPolicy GetPolicy(string uniqueProjectKey);
        byte[] GetUserImageByKey(string key);
        void AddUserImage(UserImageContext context);
        string GetUserImageKey(string username);
        TimeSpan GetOffsetFromProjectKey(string key);
    }
}
