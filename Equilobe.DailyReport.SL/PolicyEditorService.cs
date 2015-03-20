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

namespace Equilobe.DailyReport.SL
{
    public class PolicyEditorService : IPolicyEditorService
    {
        public IPolicySummaryService PolicySummaryService { get; set; }
        public ISourceControlService SourceControlService { get; set; }
        public IJiraService JiraService { get; set; }

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

            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.Single(r => r.Id == context.Id);
                // var reportPolicy = reportSettings.SerializedPolicy.GetReportPolicy();                
            }

            //var jiraPolicy = CreateNewJiraPolicyForProject(context, projectId);
            //var policySummary = PolicySummaryService.GetPolicySummary(projectId);
            //var policyDetails = GetPolicyDetails(context.BaseUrl, projectId);

            //return SyncPolicy(jiraPolicy, policySummary, policyDetails);

            return null;
        }

        JiraPolicy CreateNewJiraPolicyForProject(JiraRequestContext context, long projectId)
        {
            var project = JiraService.GetProject(context, projectId);

            var options = JiraService.GetUsers(context, project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return new JiraPolicy
            {
                BaseUrl = context.BaseUrl,
                SharedSecret = context.SharedSecret,
                Username = context.Username,
                Password = context.Password,
                ProjectId = projectId,
                UserOptions = options
            };
        }

        public ReportPolicy GetPolicyDetails(string baseUrl, long projectId)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl);

                if (reportSettings == null || reportSettings.SerializedPolicy == null)
                    return null;

                return Deserialization.XmlDeserialize<ReportPolicy>(reportSettings.SerializedPolicy.PolicyString);
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


