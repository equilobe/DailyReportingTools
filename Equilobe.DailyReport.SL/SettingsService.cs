using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Equilobe.DailyReport.SL
{
    public class SettingsService : ISettingsService
    {
        public ISourceControlService SourceControlService { get; set; }
        public IJiraService JiraService { get; set; }

        public void SetAllBasicSettings(ItemContext context)
        {
            var jiraRequestContext = new JiraRequestContext();
            new ReportsDb().InstalledInstances
                           .Where(ii => ii.Id == context.Id)
                           .Single()
                           .CopyPropertiesOnObjects(jiraRequestContext);

            var jiraProjects = JiraService.GetProjectsInfo(jiraRequestContext);

            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances
                                          .Where(ii => ii.Id == context.Id)
                                          .Single();

                jiraProjects.ForEach(jiraProject =>
                    {
                        installedInstance.BasicSettings.Add(new BasicSettings
                        {
                            InstalledInstanceId = installedInstance.Id,
                            BaseUrl = installedInstance.BaseUrl,
                            ProjectId = jiraProject.ProjectId,
                            UniqueProjectKey = RandomString.Get(jiraProject.ProjectKey)
                        });
                    });

                db.SaveChanges();
            }
        }

        public BasicReportSettings GetBasicSettings(ItemContext context)
        {
            var basicSettings = new ReportsDb().BasicSettings
                                                .Where(rs => rs.Id == context.Id)
                                                .SingleOrDefault();
            if (basicSettings == null)
                return null;

            var installedInstance = basicSettings.InstalledInstance;
            var jiraRequestContext = new JiraRequestContext();
            installedInstance.CopyPropertiesOnObjects(jiraRequestContext);

            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, basicSettings.ProjectId);
            return new BasicReportSettings
            {
                Id = context.Id,
                BaseUrl = installedInstance.BaseUrl,
                ProjectId = projectInfo.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = GetReportTime(installedInstance, projectInfo.ProjectId)
            };
        }

        public List<BasicReportSettings> GetAllBasicSettings(ItemContext context)
        {
            var installedInstance = new ReportsDb().InstalledInstances
                                                   .Where(i => i.Id == context.Id)
                                                   .Single();

            var jiraRequestContext = new JiraRequestContext();
            installedInstance.CopyPropertiesOnObjects(jiraRequestContext);

            return JiraService.GetProjectsInfo(jiraRequestContext)
                              .Select(projectInfo => new BasicReportSettings
                              {
                                  Id = GetBasicSettingsId(installedInstance, projectInfo.ProjectId),
                                  BaseUrl = installedInstance.BaseUrl,
                                  ProjectId = projectInfo.ProjectId,
                                  ProjectKey = projectInfo.ProjectKey,
                                  ProjectName = projectInfo.ProjectName,
                                  ReportTime = GetReportTime(installedInstance, projectInfo.ProjectId),
                                  UniqueProjectKey = installedInstance.BasicSettings.Where(bs => bs.ProjectId == projectInfo.ProjectId)
                                                                                    .Select(bs => bs.UniqueProjectKey)
                                                                                    .Single()
                              })
                              .ToList();
        }

        public AdvancedReportSettings GetAdvancedSettings(ItemContext context)
        {
            using (var db = new ReportsDb())
            {
                var basicSettings = db.BasicSettings.SingleOrDefault(r => r.Id == context.Id);
                if (basicSettings == null || basicSettings.SerializedAdvancedSettings == null)
                    return null;

                return Deserialization.XmlDeserialize<AdvancedReportSettings>(basicSettings.SerializedAdvancedSettings.PolicyString);
            }
        }

        public FullReportSettings GetFullSettings(ItemContext context)
        {
            var basicSettings = GetBasicSettings(context);
            var advancedSettings = GetAdvancedSettings(context);

            var fullReportSettings = new FullReportSettings();
            basicSettings.CopyPropertiesOnObjects(fullReportSettings);
            advancedSettings.CopyPropertiesOnObjects(fullReportSettings);

            return fullReportSettings;
        }

        public FullReportSettings GetSyncedSettings(ItemContext context)
        {
            var jiraInfo = JiraService.GetJiraInfo(context);
            var fullSettings = GetFullSettings(context);

            SyncUserOptions(jiraInfo, fullSettings);

            return fullSettings;
        }

        private void SyncUserOptions(JiraPolicy jiraInfo, FullReportSettings fullSettings)
        {
            SyncSourceControlUsernames(fullSettings);

            var syncedUserOptions = new List<User>();
            jiraInfo.UserOptions.ForEach(juser =>
            {
                fullSettings.UserOptions.ForEach(user =>
                {
                    if (user.JiraUserKey == juser.JiraUserKey)
                        syncedUserOptions.Add(user);
                });

                if (!syncedUserOptions.Exists(user => user.JiraUserKey == juser.JiraUserKey))
                    syncedUserOptions.Add(juser);
            });
            fullSettings.UserOptions = syncedUserOptions;
        }

        private void SyncSourceControlUsernames(FullReportSettings fullSettings)
        {
            fullSettings.SourceControlUsernames = SourceControlService.GetContributors(fullSettings.SourceControlOptions);
            fullSettings.UserOptions.ForEach(user =>
            {
                var validSourceControlUsernames = new List<string>();
                user.SourceControlUsernames.ForEach(userSrcName =>
                {
                    if (fullSettings.SourceControlUsernames.Exists(srcUsername => srcUsername == userSrcName))
                        validSourceControlUsernames.Add(userSrcName);
                });
                user.SourceControlUsernames = validSourceControlUsernames;
            });
        }

        private static long GetBasicSettingsId(InstalledInstance instance, long projectId)
        {
            return instance.BasicSettings
                           .Where(r => r.ProjectId == projectId)
                           .Select(r => r.Id)
                           .SingleOrDefault();
        }

        private static string GetReportTime(InstalledInstance instance, long projectId)
        {
            return instance.BasicSettings
                           .Where(r => r.ProjectId == projectId)
                           .Select(r => r.ReportTime)
                           .SingleOrDefault();
        }
    }
}


