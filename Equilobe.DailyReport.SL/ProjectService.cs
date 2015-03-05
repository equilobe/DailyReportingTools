using Equilobe.DailyReport.Models.Interfaces;
using System.IO;

namespace Equilobe.DailyReport.SL
{
    public class ProjectService
    {
        public static string GetUniqueProjectKey(string key)
        {
            return key + Path.GetRandomFileName().Replace(".", string.Empty);
        }
    }
}
