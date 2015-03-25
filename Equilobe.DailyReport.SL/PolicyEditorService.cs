﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System.Collections.Specialized;
using System.Linq;
using System;
using Equilobe.DailyReport.Models;
using Encryptamajig;

namespace Equilobe.DailyReport.SL
{
    public class PolicyEditorService : IPolicyEditorService
    {
        public IPolicySummaryService PolicySummaryService { get; set; }
        public ISourceControlService SourceControlService { get; set; }
        public IJiraService JiraService { get; set; }
        public IDataService DataService { get; set; }

        public string GetJiraBaseUrl(NameValueCollection queryString)
        {
            var baseUrl = queryString["xdm_e"] + queryString["cp"];
            return baseUrl;
        }

        public string GetJiraUsername(NameValueCollection queryString)
        {
            var userId = queryString["user_id"];
            return userId;
        }

        public FullReportSettings GetPolicy(ItemContext context)
        {
            var jiraPolicy = GetJiraPolicy(context);
            var policySummary = PolicySummaryService.GetPolicySummary(context);
            var policyDetails = GetPolicyDetails(context);

            return SyncPolicy(jiraPolicy, policySummary, policyDetails);
        }

        public JiraPolicy GetJiraPolicy(ItemContext context)
        {
            var reportSettings = new ReportsDb().BasicSettings.SingleOrDefault(r => r.Id == context.Id);
            if (reportSettings == null)
                return null;

            var instance = reportSettings.InstalledInstance;
            var jiraContext = new JiraRequestContext();
            instance.CopyPropertiesOnObjects(jiraContext);
            jiraContext.JiraPassword = AesEncryptamajig.Decrypt(jiraContext.JiraPassword, DataService.GetEncriptedKey());

            var project = JiraService.GetProject(jiraContext, reportSettings.ProjectId);

            var options = JiraService.GetUsers(jiraContext, project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return new JiraPolicy
            {
                BaseUrl = jiraContext.BaseUrl,
                Username = jiraContext.JiraUsername,
                Password = jiraContext.JiraPassword,
                ProjectId = reportSettings.ProjectId,
                UserOptions = options
            };
        }

        public ReportPolicy GetPolicyDetails(ItemContext context)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.BasicSettings.SingleOrDefault(r => r.Id == context.Id);
                if (reportSettings == null || reportSettings.SerializedAdvancedSettings == null)
                    return null;

                return Deserialization.XmlDeserialize<ReportPolicy>(reportSettings.SerializedAdvancedSettings.PolicyString);
            }
        }

        public FullReportSettings SyncPolicy(JiraPolicy jiraPolicy, ReportSettingsSummary policySummary, ReportPolicy policyDetails)
        {
            var policyBuffer = new FullReportSettings();
            jiraPolicy.CopyPropertiesOnObjects(policyBuffer);

            policySummary.CopyPropertiesOnObjects(policyBuffer);

            policyDetails.CopyPropertiesOnObjects(policyBuffer);

            if (policyBuffer.SourceControlOptions != null)
                policyBuffer.SourceControlUsernames = SourceControlService.GetContributors(policyBuffer.SourceControlOptions);

            return policyBuffer;
        }
    }
}


