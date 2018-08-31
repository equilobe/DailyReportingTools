using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Storage;
using System.Linq;

namespace Equilobe.DailyReport.SL.DbExtensions
{
    public static class AtlassianUserExtensions
    {
        public static IQueryable<AtlassianUser> GetAtlassianUsers(this ReportsDb db, long instanceId, bool? isActive = null, bool? isStalling = null)
        {
            return db.AtlassianUsers
                .Where(p => p.InstalledInstanceId == instanceId)
                .Where(p => !isActive.HasValue || p.IsActive == isActive)
                .Where(p => !isStalling.HasValue || p.IsStalling == isStalling);
        }
    }
}
