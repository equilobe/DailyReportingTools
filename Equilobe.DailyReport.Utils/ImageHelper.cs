using System;

namespace Equilobe.DailyReport.Utils
{
    public static class ImageHelper
    {
        public static string GetUserAvatarsFullPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "Content\\Images\\UserAvatars";
        }

        public static string GetUserAvatarsRelativePath()
        {
            return "../Content/Images/UserAvatars";
        }
    }
}
