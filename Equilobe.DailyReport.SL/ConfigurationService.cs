using Equilobe.DailyReport.Models.Interfaces;
using System;
using System.Configuration;
using System.Text;
using System.Web.Configuration;

namespace Equilobe.DailyReport.SL
{
    public class ConfigurationService : IConfigurationService
    {
        public string GetAddonKey()
        {
            return ConfigurationManager.AppSettings["addonKey"];
        }

        public string GetWebBaseUrl()
        {
            return ConfigurationManager.AppSettings["webBaseUrl"];
        }

        public string GetEncriptedKey()
        {
            var byteKey = Convert.FromBase64String(ConfigurationManager.AppSettings["encKey"]);
            var encKey = Encoding.UTF8.GetString(byteKey);
            return encKey;
        }

        public string GetJiraReporterPath()
        {
            return ConfigurationManager.AppSettings["jiraReporterPath"];
        }

        public string GetReportToolPath()
        {
            return ConfigurationManager.AppSettings["reportToolPath"]; 
        }

        public string GetTaskSchedulerFolderName()
        {
            return ConfigurationManager.AppSettings["taskSchedulerFolderName"];
        }

        public string GetTimeZoneMappingPath()
        {
            return WebConfigurationManager.AppSettings["TimeZoneMappingPath"];
        }

        public bool IsPaypalSandbox()
        {
            return Convert.ToBoolean(ConfigurationManager.AppSettings["isPaypalSandbox"]);
        }

        public string GetBitBucketApiClientUrl()
        {
            return ConfigurationManager.AppSettings["bitBucketApiClientUrl"];
        }

        public string GetDashboardDataSyncScriptPath()
        {
            return ConfigurationManager.AppSettings["dashboardDataSyncScriptPath"];
        }

        public string GetSyncTaskScriptPath()
        {
            return ConfigurationManager.AppSettings["syncTaskScriptPath"];
        }
    }
}
