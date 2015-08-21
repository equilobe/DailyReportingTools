using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.General;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security;

namespace Equilobe.DailyReport.SL
{
    public class SettingsService : ISettingsService
    {
        public ISourceControlService SourceControlService { get; set; }
        public IJiraService JiraService { get; set; }
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public void SyncAllBasicSettings(ItemContext context)
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
                               if (installedInstance.BasicSettings.Where(bs => bs.ProjectId == jiraProject.ProjectId).SingleOrDefault() == null)
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
            var jiraRequestContext = new JiraRequestContext();
            var reportSettings = new BasicReportSettings();

            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                var basicSettings = db.BasicSettings
                                      .Where(bs => bs.InstalledInstance.UserId == userId && bs.Id == context.Id)
                                      .SingleOrDefault();

                if (basicSettings == null)
                    return null;

                basicSettings.CopyPropertiesOnObjects(reportSettings);
                basicSettings.InstalledInstance.CopyPropertiesOnObjects(jiraRequestContext);
            }

            var projectInfo = JiraService.GetProjectInfo(jiraRequestContext, reportSettings.ProjectId);
            return new BasicReportSettings
            {
                Id = reportSettings.Id,
                BaseUrl = reportSettings.BaseUrl,
                ProjectId = reportSettings.ProjectId,
                ProjectKey = projectInfo.ProjectKey,
                UniqueProjectKey = reportSettings.UniqueProjectKey,
                ProjectName = projectInfo.ProjectName,
                ReportTime = reportSettings.ReportTime
            };
        }

        public List<BasicReportSettings> GetAllBasicReportSettings(ItemContext context)
        {
            var baseUrl = string.Empty;
            var jiraRequestContext = new JiraRequestContext();

            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                var instance = db.InstalledInstances.Where(i => i.UserId == userId && i.Id == context.Id).Single();

                instance.CopyPropertiesOnObjects(jiraRequestContext);
                baseUrl = instance.BaseUrl;

                if (instance.ExpirationDate <= DateTime.Now)
                    return null;
            }

            return JiraService.GetProjectsInfo(jiraRequestContext)
                              .Select(projectInfo =>
                              {
                                  var basicSettings = GetBasicSettings(context, projectInfo.ProjectId);
                                  if (basicSettings == null)
                                  {
                                      SyncAllBasicSettings(context);
                                      basicSettings = GetBasicSettings(context, projectInfo.ProjectId);
                                  }

                                  return new BasicReportSettings
                                  {
                                      Id = basicSettings.Id,
                                      InstalledInstanceId = basicSettings.InstalledInstanceId,
                                      BaseUrl = baseUrl,
                                      ProjectId = basicSettings.ProjectId,
                                      ProjectKey = projectInfo.ProjectKey,
                                      UniqueProjectKey = basicSettings.UniqueProjectKey,
                                      ProjectName = projectInfo.ProjectName,
                                      ReportTime = basicSettings.ReportTime
                                  };
                              })
                              .ToList();
        }

        public List<JiraInstance> GetAllBasicReportSettings(UserContext context)
        {
            List<InstalledInstance> installedInstances;
            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                installedInstances = db.InstalledInstances
                                       .Where(i => i.UserId == userId && i.UserId == context.UserId)
                                       .ToList();
                if (installedInstances.Count == 0)
                    return null;
            }

            var instances = new List<JiraInstance>();

            foreach (var installedInstance in installedInstances)
            {              
                var icontext = new ItemContext(installedInstance.Id);
                var jiraRequestContext = new JiraRequestContext(installedInstance);
                var instance = new JiraInstance
                {
                    BaseUrl = installedInstance.BaseUrl,
                    Id = installedInstance.Id,
                    ExpirationDate = installedInstance.ExpirationDate
                };

                instances.Add(instance);

                if (installedInstance.ExpirationDate <= DateTime.Now)                   
                    continue;                

                var projects = JiraService.GetProjectsInfo(jiraRequestContext)
                                          .Select(projectInfo =>
                                          {
                                              var basicSettings = GetBasicSettings(icontext, projectInfo.ProjectId);
                                              if (basicSettings == null)
                                              {
                                                  SyncAllBasicSettings(icontext);
                                                  basicSettings = GetBasicSettings(icontext, projectInfo.ProjectId);
                                              }

                                              return new BasicReportSettings
                                              {
                                                  Id = basicSettings.Id,
                                                  InstalledInstanceId = basicSettings.InstalledInstanceId,
                                                  BaseUrl = UrlExtensions.GetAuthority(installedInstance.BaseUrl),
                                                  ProjectId = basicSettings.ProjectId,
                                                  ProjectKey = projectInfo.ProjectKey,
                                                  UniqueProjectKey = basicSettings.UniqueProjectKey,
                                                  ProjectName = projectInfo.ProjectName,
                                                  ReportTime = basicSettings.ReportTime
                                              };
                                          })
                                          .ToList();
                instance.Projects = projects;
            }

            return instances;
        }

        public AdvancedReportSettings GetAdvancedReportSettings(ItemContext context)
        {
            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                var serializedAdvancedSettings = db.BasicSettings
                                                   .Where(bs => bs.InstalledInstance.UserId == userId && bs.Id == context.Id)
                                                   .Single()
                                                   .SerializedAdvancedSettings;
                if (serializedAdvancedSettings != null)
                    return Deserialization.XmlDeserialize<AdvancedReportSettings>(serializedAdvancedSettings.PolicyString);
            }
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

        public void SetFullReportSettings(FullReportSettings updatedFullSettings)
        {
            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                var basicSettings = db.BasicSettings.Where(bs => bs.InstalledInstance.UserId == userId && bs.UniqueProjectKey == updatedFullSettings.UniqueProjectKey).Single();

                if (basicSettings.ReportTime != updatedFullSettings.ReportTime)
                {
                    basicSettings.ReportTime = updatedFullSettings.ReportTime;
                    TaskSchedulerService.SetTask(new ScheduledTaskContext
                    {
                        ReportTime = updatedFullSettings.ReportTime,
                        UniqueProjectKey = updatedFullSettings.UniqueProjectKey
                    });
                }

                var advancedSettings = new AdvancedReportSettings();
                updatedFullSettings.CopyTo<IAdvancedSettings>(advancedSettings);

                if (basicSettings.SerializedAdvancedSettings == null)
                    basicSettings.SerializedAdvancedSettings = new SerializedAdvancedSettings();
                basicSettings.SerializedAdvancedSettings.PolicyString = Serialization.XmlSerialize(advancedSettings);

                db.SaveChanges();
            }
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
            using (var db = new ReportsDb())
            {
                var userId = new UserContext().UserId;
                return db.InstalledInstances
                                  .Where(ii => ii.UserId == userId && ii.Id == context.Id)
                                  .Single()
                                  .BasicSettings
                                  .Where(bs => bs.ProjectId == projectId)
                                  .SingleOrDefault();
            }
        }
    }
}


