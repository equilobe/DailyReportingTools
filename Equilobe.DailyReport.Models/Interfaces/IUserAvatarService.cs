using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IUserAvatarService : IService
    {
        Dictionary<string, string> UploadUsersAvatarsAndGetFilenames(List<AtlassianUser> users, ReportContext context);
    }
}
