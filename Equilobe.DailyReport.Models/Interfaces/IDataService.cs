using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IDataService : IService
    {
        void SaveInstance(InstalledInstance instanceData);
        void SaveInstance(RegisterModel modelData);
        void DeleteInstance(string baseUrl);
        List<Instance> GetInstances();
        long GetNumberOfReportsGenerated();
        string GetBaseUrl(long id);
        string GetSharedSecret(string baseUrl);
        string GetPassword(string baseUrl, string username);
        string GetReportTime(string baseUrl, long projectId);
        System.Collections.Generic.List<string> GetUniqueProjectsKey(string baseUrl);
        void SetReportFromDb(Equilobe.DailyReport.Models.ReportFrame.JiraReport report);
        JiraPolicy GetPolicy(string uniqueProjectKey);
        byte[] GetImage(string key);
    }
}
