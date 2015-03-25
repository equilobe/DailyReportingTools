﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models;
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
        public IDataService DataService { get; set; }

        public BasicReportSettings GetBasicSettings(ItemContext context)
        {
            var basicSettings = new ReportsDb().BasicSettings
                                                .Where(rs => rs.Id == context.Id)
                                                .SingleOrDefault();
            if (basicSettings == null)
                return null;

            var installedInstance = basicSettings.InstalledInstance;
            if (installedInstance.UserId != context.UserId)
                throw new SecurityException();

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

            if (installedInstance.UserId != context.UserId)
                throw new SecurityException();

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
                                  ReportTime = GetReportTime(installedInstance, projectInfo.ProjectId)
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
            var jiraInfo = JiraService.GetJiraInfo(context);
            var basicSettings = GetBasicSettings(context);
            var advancedSettings = GetAdvancedSettings(context);

            return SyncSettings(jiraInfo, basicSettings, advancedSettings);
        }

        private FullReportSettings SyncSettings(JiraPolicy jiraInfo, BasicReportSettings basicSettings, AdvancedReportSettings advancedSettings)
        {
            var fullSettings = new FullReportSettings();
            jiraInfo.CopyPropertiesOnObjects(fullSettings);

            basicSettings.CopyPropertiesOnObjects(fullSettings);

            advancedSettings.CopyPropertiesOnObjects(fullSettings);

            if (fullSettings.SourceControlOptions != null)
                fullSettings.SourceControlUsernames = SourceControlService.GetContributors(fullSettings.SourceControlOptions);

            return fullSettings;
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


