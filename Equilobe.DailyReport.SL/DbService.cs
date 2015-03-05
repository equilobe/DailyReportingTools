using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Storage;
using Newtonsoft.Json;
using System.Linq;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class DbService
    {
        public static void SaveSharedSecret(HttpRequestBase request)
        {
            var bodyText = new System.IO.StreamReader(request.InputStream).ReadToEnd();
            var instanceData = JsonConvert.DeserializeObject<InstalledInstance>(bodyText);

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == instanceData.BaseUrl);
                if (installedInstance == null)
                    db.InstalledInstances.Add(instanceData);
                else
                    installedInstance.SharedSecret = instanceData.SharedSecret;

                db.SaveChanges();
            }
        }

        public static void DeleteSharedSecret(HttpRequestBase request)
        {
            var bodyText = new System.IO.StreamReader(request.InputStream).ReadToEnd();
            var instanceData = JsonConvert.DeserializeObject<InstalledInstance>(bodyText);

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == instanceData.BaseUrl);
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public static string GetSharedSecret(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(x => x.BaseUrl == baseUrl)
                .OrderByDescending(x => x.Id)
                .Select(x => x.SharedSecret)
                .FirstOrDefault();
        }

        public static string GetReportTime(string baseUrl, long projectId)
        {
            return new ReportsDb().ReportSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
        }
    }
}
