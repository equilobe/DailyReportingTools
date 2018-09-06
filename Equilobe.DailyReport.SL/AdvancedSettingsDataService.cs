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
            var sourceControlOptions = new List<SourceControlOptions>();

            using (var db = new ReportsDb())
            {
                var policyStrings = GetInstancePolicyStrings(instanceId);

                if (!policyStrings.Any())
                    return sourceControlOptions;

                foreach (var policyString in policyStrings)
                {
                    var advancedSettings = new AdvancedReportSettings();

                    Deserialization.XmlDeserialize<AdvancedReportSettings>(policyString)
                        .CopyPropertiesOnObjects(advancedSettings);

                    sourceControlOptions.Add(advancedSettings.SourceControlOptions);
                }
            }

            return sourceControlOptions;
        }

        public List<User> GetUserMappings(long instanceId)
        {
            var users = new List<User>();

            using (var db = new ReportsDb())
            {
                var policyStrings = GetInstancePolicyStrings(instanceId);

                if (!policyStrings.Any())
                    return users;

                foreach (var policyString in policyStrings)
                {
                    var advancedSettings = new AdvancedReportSettings();

                    Deserialization.XmlDeserialize<AdvancedReportSettings>(policyString)
                        .CopyPropertiesOnObjects(advancedSettings);

                    users.AddRange(advancedSettings.UserOptions);
                }
            }

            return users
                .GroupBy(p => p.JiraUserKey)
                .Select(p => p.FirstOrDefault())
                .ToList();
        }
        #endregion

        #region Helpers
        private List<string> GetInstancePolicyStrings(long instanceId)
        {
            using (var db = new ReportsDb())
            {
                return db.BasicSettings
                    .Where(p => p.InstalledInstanceId == instanceId)
                    .Where(p => p.SerializedAdvancedSettings != null)
                    .Select(p => p.SerializedAdvancedSettings.PolicyString)
                    .ToList();
            }
        }
        #endregion
    }
}
