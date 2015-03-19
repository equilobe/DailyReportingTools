using Equilobe.DailyReport.Models.Policy;
using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IDataService : IService
    {
        void DeleteInstance(string baseUrl);
        string GetBaseUrl(long id);
        System.Collections.Generic.List<Equilobe.DailyReport.Models.Storage.InstalledInstance> GetInstances(string username);
        string GetPassword(string baseUrl, string username);
        string GetReportTime(string baseUrl, long projectId);
        string GetSharedSecret(string baseUrl);
        System.Collections.Generic.List<string> GetUniqueProjectsKey(string baseUrl);
        void SaveInstance(Equilobe.DailyReport.Models.Storage.InstalledInstance instanceData);
        void SetReportFromDb(Equilobe.DailyReport.Models.ReportFrame.JiraReport report);
        JiraPolicy GetPolicy(string uniqueProjectKey);
    }
}
