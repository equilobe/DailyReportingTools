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

namespace Equilobe.DailyReport.SL
{
    public class PolicyEditorService : IPolicyEditorService
    {
        public IJiraRequestContextService JiraRequestContextService { get; set; }
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

        public PolicyBuffer GetPolicy(long projectId)
        {
            var jiraPolicy = CreateNewJiraPolicyForProject(projectId);
            var policySummary = PolicySummaryService.GetPolicySummary(projectId);
            var policyDetails = GetPolicyDetails(JiraRequestContextService.Context.BaseUrl, projectId);

            return SyncPolicy(jiraPolicy, policySummary, policyDetails);
        }

        public JiraPolicy CreateNewJiraPolicyForProject(long projectId)
        {
            var project = JiraService.GetProject(projectId);

            var options = JiraService.GetUsers(project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return new JiraPolicy
            {
                BaseUrl = JiraRequestContextService.Context.BaseUrl,
                SharedSecret = JiraRequestContextService.Context.SharedSecret,
                Username = JiraRequestContextService.Context.Username,
                Password = JiraRequestContextService.Context.Password,
                ProjectId = projectId,
                UserOptions = options
            };
        }

        public PolicyDetails GetPolicyDetails(string baseUrl, long projectId)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.ProjectId == projectId && qr.BaseUrl == baseUrl);

                if (reportSettings == null || reportSettings.SerializedPolicy == null)
                    return null;

                return Deserialization.XmlDeserialize<PolicyDetails>(reportSettings.SerializedPolicy.PolicyString);
            }
        }

        public PolicyBuffer SyncPolicy(JiraPolicy jiraPolicy, PolicySummary policySummary, PolicyDetails policyDetails)
        {
            var policyBuffer = new PolicyBuffer();
            jiraPolicy.CopyPropertiesOnObjects(policyBuffer);

            policySummary.CopyPropertiesOnObjects(policyBuffer);

            policyDetails.CopyPropertiesOnObjects(policyBuffer);

            if (policyBuffer.SourceControlOptions != null)
                policyBuffer.SourceControlUsernames = SourceControlService.GetContributors(policyBuffer.SourceControlOptions);

            return policyBuffer;
        }

      
    }
}


