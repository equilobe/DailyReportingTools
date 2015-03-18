﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class DataService
    {
        public void SaveInstance(InstalledInstance instanceData)
        {
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

        public void DeleteInstance(string baseUrl)
        {
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances.SingleOrDefault(qr => qr.BaseUrl == baseUrl);
                db.InstalledInstances.Remove(installedInstance);

                db.SaveChanges();
            }
        }

        public string GetSharedSecret(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(x => x.BaseUrl == baseUrl)
                .Select(x => x.SharedSecret)
                .FirstOrDefault();
        }

        public string GetPassword(string baseUrl, string username)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.BaseUrl == baseUrl)
                .SelectMany(installedInstance => installedInstance.ReportSettings
                    .Where(reportSettings => reportSettings.Username == username)
                    .Select(reportSettings => reportSettings.Password))
                .FirstOrDefault();
        }

        public string GetReportTime(string baseUrl, long projectId)
        {
            return new ReportsDb().ReportSettings
                .Where(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl)
                .Select(qr => qr.ReportTime)
                .FirstOrDefault();
        }

        public List<string> GetUniqueProjectsKey(string baseUrl)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.BaseUrl == baseUrl)
                .SelectMany(installedInstance => installedInstance.ReportSettings
                .Select(reportSettings => reportSettings.UniqueProjectKey))
                .ToList();
        }

        public List<Instance> GetInstances(ApplicationUser user)
        {
            var installedInstances = user.InstalledInstances.ToList();
            var instances = new List<Instance>();

            installedInstances.ForEach(installedInstance =>
                {
                    instances.Add(new Instance { 
                        Id = installedInstance.Id,
                        BaseUrl = installedInstance.BaseUrl
                    });
                });

            return instances;
        }

        public string GetBaseUrl(long id)
        {
            return new ReportsDb().InstalledInstances
                .Where(installedInstance => installedInstance.Id == id)
                .Select(installedInstance => installedInstance.BaseUrl)
                .FirstOrDefault();
        }
    }
}
