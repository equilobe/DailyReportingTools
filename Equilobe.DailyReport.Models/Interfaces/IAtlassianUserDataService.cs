using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IAtlassianUserDataService : IService
    {
        List<AtlassianUser> GetAtlassianUsers(long instanceId, bool? isActive = null, bool? isStalling = null);
        Dictionary<string, long> GetUserIdsByUserKeys(List<string> userKeys);
        void SyncAtlassianUsers(List<AtlassianUser> users, ReportContext context);
    }
}
