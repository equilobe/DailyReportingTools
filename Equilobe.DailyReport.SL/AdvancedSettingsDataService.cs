using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class AdvancedSettingsDataService : IAdvancedSettingsDataService
    {
        #region IAdvancedSettingsDataService implementation
        public List<SourceControlOptions> GetAllReposSourceControlOptions(long instanceId)
        {
            var advancedReportSettings = GetInstancePolicyStrings(instanceId);

            return advancedReportSettings
                .Select(p => p.SourceControlOptions)
                .ToList();
        }

        public List<User> GetUserMappings(long instanceId)
        {
            var advancedReportSettings = GetInstancePolicyStrings(instanceId);

            return advancedReportSettings
                .SelectMany(p => p.UserOptions)
                .GroupBy(p => p.JiraUserKey)
                .Select(p => ToUserMappings(p.ToList()))
                .ToList();
        }
        #endregion

        #region Helpers
        private List<AdvancedReportSettings> GetInstancePolicyStrings(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                var policies = db.BasicSettings
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Where(p => p.SerializedAdvancedSettings != null)
                    .Select(p => p.SerializedAdvancedSettings.PolicyString)
                    .ToList();

                return policies
                    .Where(p => p.Any())
                    .Select(XmlHelper.DeserializeXml<AdvancedReportSettings>)
                    .ToList();
            }
        }

        private User ToUserMappings(List<User> userGroup)
        {
            var elem = userGroup.First();

            var sourceControlUsernames = userGroup
                .SelectMany(p => p.SourceControlUsernames)
                .ToList();

            return new User
            {
                JiraUserKey = elem.JiraUserKey,
                JiraDisplayName = elem.JiraDisplayName,
                SourceControlUsernames = sourceControlUsernames
            };
        }
        #endregion
    }
}
