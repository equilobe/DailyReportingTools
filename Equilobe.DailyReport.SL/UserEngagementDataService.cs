using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class UserEngagementDataService : IUserEngagementDataService
    {
        #region IUserEngagementDataService implementation
        public void UpdateUserEngagementStats(Dictionary<long, UserEngagement> engagement, DateTime day)
        {
            using (var db = new ReportsDb())
            {
                var userIds = engagement.Keys.ToList();
                var date = day.Date;

                var dbEngagement = db.UserEngagementStats
                    .Where(p => userIds.Contains(p.AtlassianUserId))
                    .Where(p => p.Date >= day);

                if (!dbEngagement.Any())
                    AddEngagementStats(db, engagement, day);
                else
                    UpdateEngagementStats(dbEngagement, engagement);

                db.SaveChanges();
            }
        }
        #endregion

        #region Helpers
        private void AddEngagementStats(ReportsDb db, Dictionary<long, UserEngagement> engagement, DateTime day)
        {
            foreach (var eng in engagement)
            {
                var dbEng = new UserEngagementStats
                {
                    AtlassianUserId = eng.Key,
                    CommentsCount = eng.Value.CommentsCount,
                    Date = day
                };

                db.UserEngagementStats.Add(dbEng);
            }
        }

        private void UpdateEngagementStats(IQueryable<UserEngagementStats> dbEngagement, Dictionary<long, UserEngagement> engagement)
        {
            foreach (var dbEng in dbEngagement)
            {
                var userEngagement = engagement[dbEng.AtlassianUserId];

                dbEng.CommentsCount = userEngagement.CommentsCount;
            }
        }
        #endregion
    }
}
