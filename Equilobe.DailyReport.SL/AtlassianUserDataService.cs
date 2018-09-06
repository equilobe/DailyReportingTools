using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class AtlassianUserDataService : IAtlassianUserDataService
    {
        public IJiraService JiraService { get; set; }
        public IAtlassianWorklogDataService AtlassianWorklogDataService { get; set; }
        public IUserAvatarService UserAvatarService { get; set; }

        #region IAtlassianDataService implementation
        public List<AtlassianUser> GetAtlassianUsers(long instanceId, bool? isActive = null, bool? isStalling = null)
        {
            using (var db = new ReportsDb())
            {
                return db.AtlassianUsers
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Where(p => !isActive.HasValue || p.IsActive == isActive)
                    .Where(p => !isStalling.HasValue || p.IsStalling == isStalling)
                    .ToList();
            }
        }

        public void SyncAtlassianUsers(List<AtlassianUser> users, ReportContext context)
        {
            UpdateAtlassianUsers(context.InstanceId, users);
            UpdateUsersAvatars(users, context);
            UpdateStallingUsers(context);
        }
        #endregion

        #region Update methods
        private void UpdateAtlassianUsers(long instanceId, List<AtlassianUser> users)
        {
            using (var db = new ReportsDb())
            {
                var dbUsers = db.AtlassianUsers
                    .Where(p => p.InstalledInstanceId == instanceId);

                foreach (var user in users)
                {
                    var dbUser = dbUsers.Where(p => p.Key == user.Key).SingleOrDefault();

                    if (dbUser == null)
                        db.AtlassianUsers.Add(user);
                    else
                        UpdateDbUser(dbUser, user);
                }

                db.SaveChanges();
            }
        }

        private void UpdateUsersAvatars(List<AtlassianUser> users, ReportContext context)
        {
            var fileNames = UserAvatarService.UploadUsersAvatarsAndGetFilenames(users, context);

            UpdateDbUsersAvatars(fileNames, context.InstanceId);
        }

        private void UpdateDbUsersAvatars(Dictionary<string, string> fileNames, long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var users = db.AtlassianUsers.Where(p => p.InstalledInstanceId == instanceId);

                foreach (var user in users)
                {
                    var fileName = fileNames[user.Key];

                    user.AvatarFileName = fileName;
                }

                db.SaveChanges();
            }
        }

        private void UpdateStallingUsers(ReportContext context)
        {
            using (var db = new ReportsDb())
            {
                var users = db.AtlassianUsers.Where(p => p.InstalledInstanceId == context.InstanceId);
                var usersIds = users.Select(p => p.Id).ToList();
                var worklogs = AtlassianWorklogDataService.GetAtlassianWorklogsByUserIds(context.InstanceId, usersIds);

                foreach (var user in users)
                {
                    var from = DateTime.UtcNow.AddMonths(-1).ToOriginalTimeZone(context.OffsetFromUtc);
                    var worklogList = worklogs.FirstOrDefault(p => p.Id == user.Id);

                    user.IsStalling = worklogList == null ?
                        true :
                        !HasWorklogsFromTimeAgo(worklogList.Worklogs, from, context.OffsetFromUtc);
                }

                db.SaveChanges();
            }
        }
        #endregion

        #region Helpers
        private void UpdateDbUser(AtlassianUser dbUser, AtlassianUser jiraUser)
        {
            dbUser.DisplayName = jiraUser.DisplayName;
            dbUser.Key = jiraUser.Key;
            dbUser.EmailAddress = jiraUser.EmailAddress;
            dbUser.AvatarFileName = jiraUser.AvatarFileName;
            dbUser.IsActive = jiraUser.IsActive;
        }

        private bool HasWorklogsFromTimeAgo(List<AtlassianWorklog> worklogs, DateTime from, TimeSpan offsetFromUtc)
        {
            if (worklogs == null || !worklogs.Any())
                return false;

            return worklogs
                .Where(p => p.StartedAt.ToOriginalTimeZone(offsetFromUtc) > from)
                .Any();
        }
        #endregion
    }
}
