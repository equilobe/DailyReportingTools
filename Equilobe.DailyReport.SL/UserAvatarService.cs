using Equilobe.DailyReport.Models.Dashboard;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using System.Collections.Generic;
using System.IO;

namespace Equilobe.DailyReport.SL
{
    public class UserAvatarService : IUserAvatarService
    {
        public IJiraService JiraService { get; set; }

        public Dictionary<string, string> UploadUsersAvatarsAndGetFilenames(List<AtlassianUser> users, ReportContext context)
        {
            var folderPath = ImageHelper.GetUserAvatarsFullPath();
            var userFileDict = new Dictionary<string, string>();

            foreach (var user in users)
            {
                if (!user.IsActive)
                    continue;

                var image = JiraService.GetUserAvatar(context.JiraRequestContext, user.AvatarFileName);
                var imageName = user.Key + ".jpg";
                var path = Path.Combine(folderPath, imageName);

                try
                {
                    File.WriteAllBytes(path, image);
                    userFileDict.Add(user.Key, imageName);
                }
                catch
                {
                    userFileDict.Add(user.Key, null);
                }
            }

            return userFileDict;
        }
    }
}
