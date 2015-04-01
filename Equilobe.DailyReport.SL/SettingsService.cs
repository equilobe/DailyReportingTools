﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            using (var db = new ReportsDb())
            {
                var installedInstance = db.InstalledInstances
                                          .Where(ii => ii.Id == context.Id)
                                          .Single();

                var jiraRequestContext = new JiraRequestContext(installedInstance);
                JiraService.GetProjectsInfo(jiraRequestContext)
                           .ForEach(jiraProject =>
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

        public BasicReportSettings GetBasicReportSettings(ItemContext context)
        {
            var basicSettings = new ReportsDb().BasicSettings
                                               .Where(bs => bs.Id == context.Id)
                                               .SingleOrDefault();
            if (basicSettings == null)
                return null;

            var installedInstance = basicSettings.InstalledInstance;
            var jiraRequestContext = new JiraRequestContext(installedInstance);
            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, basicSettings.ProjectId);

            return new BasicReportSettings
            {
                Id = basicSettings.Id,
                BaseUrl = installedInstance.BaseUrl,
                ProjectId = basicSettings.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                UniqueProjectKey = basicSettings.UniqueProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = basicSettings.ReportTime
            };
        }

        public List<BasicReportSettings> GetAllBasicReportSettings(ItemContext context)
        {
            var installedInstance = new ReportsDb().InstalledInstances
                                                   .Where(i => i.Id == context.Id)
                                                   .Single();
            var jiraRequestContext = new JiraRequestContext(installedInstance);

            return JiraService.GetProjectsInfo(jiraRequestContext)
                              .Select(projectInfo =>
                                  {
                                      var basicSettings = GetBasicSettings(context, projectInfo.ProjectId);
                                      return new BasicReportSettings
                                      {
                                          Id = basicSettings.Id,
                                          InstalledInstanceId = basicSettings.InstalledInstanceId,
                                          BaseUrl = installedInstance.BaseUrl,
                                          ProjectId = basicSettings.ProjectId,
                                          ProjectKey = projectInfo.ProjectKey,
                                          UniqueProjectKey = basicSettings.UniqueProjectKey,
                                          ProjectName = projectInfo.ProjectName,
                                          ReportTime = basicSettings.ReportTime
                                      };
                                  })
                                  .ToList();
        }

        public AdvancedReportSettings GetAdvancedReportSettings(ItemContext context)
        {
            var serializedAdvancedSettings = new ReportsDb().BasicSettings
                                                            .Single(bs => bs.Id == context.Id)
                                                            .SerializedAdvancedSettings;
            if (serializedAdvancedSettings != null)
                return Deserialization.XmlDeserialize<AdvancedReportSettings>(serializedAdvancedSettings.PolicyString);

            return GetDefaultAdvancedReportSettings();
        }

        public AdvancedReportSettings GetDefaultAdvancedReportSettings()
        {
            var advancedReportSettings = new AdvancedReportSettings
            {
                UserOptions = new List<User>(),
                MonthlyOptions = new List<Month>(),
                AdvancedOptions = new AdvancedOptions
                {
                    WeekendDaysList = new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday },
                }
            };

            advancedReportSettings.MonthlyOptions.AddRange(DateTimeFormatInfo.InvariantInfo.MonthNames.ToList()
                .Where(monthName => !string.IsNullOrEmpty(monthName))
                .ToList()
                .Select(monthName => new Month { MonthName = monthName })
                .ToList());

            return advancedReportSettings;
        }

        public FullReportSettings GetFullReportSettings(ItemContext context)
        {
            var basicSettings = GetBasicReportSettings(context);
            var advancedSettings = GetAdvancedReportSettings(context);

            var fullReportSettings = new FullReportSettings();
            basicSettings.CopyPropertiesOnObjects(fullReportSettings);
            advancedSettings.CopyPropertiesOnObjects(fullReportSettings);

            return fullReportSettings;
        }

        public FullReportSettings GetSyncedReportSettings(ItemContext context)
        {
            var jiraInfo = JiraService.GetJiraInfo(context);
            var fullSettings = GetFullReportSettings(context);

            SyncUserOptions(jiraInfo, fullSettings);

            return fullSettings;
        }

        private void SyncUserOptions(JiraPolicy jiraInfo, FullReportSettings fullSettings)
        {
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

            SyncSourceControlUsernames(fullSettings);
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

        private BasicSettings GetBasicSettings(ItemContext context, long projectId)
        {
            return new ReportsDb().InstalledInstances
                                  .Where(ii => ii.Id == context.Id)
                                  .Single()
                                  .BasicSettings
                                  .Where(bs => bs.ProjectId == projectId)
                                  .Single();
        }
    }
}

