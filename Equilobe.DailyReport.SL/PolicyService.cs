﻿using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System.Collections.Specialized;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class PolicyService
    {
        public string _baseUrl { get; set; }
        public string _sharedSecret { get; set; }

        public PolicyService()
        {

        }

        public PolicyService(string baseUrl, string sharedSecret)
        {
            _baseUrl = baseUrl;
            _sharedSecret = sharedSecret;
        }


        public string GetJiraBaseUrl(NameValueCollection queryString)
        {
            var baserUrl = queryString["xdm_e"] + queryString["cp"];
            return baserUrl;
        }

        public string GetJiraUsername(NameValueCollection queryString)
        {
            var userId = queryString["user_id"];
            return userId;
        }

        public PolicyBuffer GetPolicy(long projectId)
        {
            var jiraPolicy = GetJiraPolicy(projectId);
            var policySummary = new PolicySummaryService(_baseUrl, _sharedSecret).GetPolicySummary(projectId);
            var policyDetails = GetPolicyDetails(_baseUrl, projectId);

            return SyncPolicy(jiraPolicy, policySummary, policyDetails);
        }

        public JiraPolicy GetJiraPolicy(long projectId)
        {
            var context = new JiraRequestContext
            {
                BaseUrl = _baseUrl,
                SharedSecret = _sharedSecret
            };

            var policy = new JiraPolicy
            {
                BaseUrl = _baseUrl,
                SharedSecret = _sharedSecret,
                ProjectId = projectId
            };

            var project = new JiraService().GetProject(context, projectId);

            policy.UserOptions = new JiraService().GetUsers(context, project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return policy;
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
            jiraPolicy.CopyProperties(policyBuffer);

            policySummary.CopyProperties(policyBuffer);

            policyDetails.CopyProperties(policyBuffer);

            if (policyBuffer.SourceControlOptions != null)
                policyBuffer.SourceControlUsernames = new SourceControlService().GetContributors(policyBuffer.SourceControlOptions);

            return policyBuffer;
        }

        public PolicyBuffer GetPolicyBufferFromDb(string uniqueProjectKey)
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == uniqueProjectKey);

                if (reportSettings == null || reportSettings.SerializedPolicy == null)
                    return null;

                var policyBuffer = new PolicyBuffer();
                reportSettings.CopyProperties<IReportSetting>(policyBuffer);

                if (!string.IsNullOrEmpty(reportSettings.SerializedPolicy.PolicyString))
                    Deserialization.XmlDeserialize<PolicyDetails>(reportSettings.SerializedPolicy.PolicyString)
                        .CopyProperties(policyBuffer);

                return policyBuffer;
            }
        }

        public JiraPolicy GetPolicy(string uniqueProjectKey)
        {
            var policyBuffer = GetPolicyBufferFromDb(uniqueProjectKey);
            var policy = new JiraPolicy();
            policyBuffer.CopyProperties(policy);
            return policy;
        }
    }
}


