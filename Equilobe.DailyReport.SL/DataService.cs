using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Equilobe.DailyReport.SL
{
    public class DataService
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

        public static void DeleteSharedSecret(string baseUrl)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == baseUrl);
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public static string GetSharedSecret(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(x => x.BaseUrl == baseUrl)
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

        public static List<string> GetUniqueProjectsKey(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.BaseUrl == baseUrl)
                .SelectMany(installedInstance => installedInstance.ReportSettings
                .Select(reportSettings => reportSettings.UniqueProjectKey))
                .ToList();
        }
    }
}
