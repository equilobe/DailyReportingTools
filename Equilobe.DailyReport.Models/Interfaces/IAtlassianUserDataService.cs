using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IAtlassianUserDataService : IService
    {
        List<AtlassianUser> GetAtlassianUsers(long instanceId, bool? isActive = null, bool? isStalling = null);
        void SyncAtlassianUsers(List<AtlassianUser> users, ReportContext context);
    }
}
