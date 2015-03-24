﻿﻿using Equilobe.DailyReport.DAL;
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
        public JiraRequestContext _requestContext { get; set; }

        public PolicyService()
        {
        }

        public PolicyService(JiraRequestContext context)
        {
            _requestContext = context;
        }

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
            var jiraPolicy = GetJiraPolicy(projectId);
            var policySummary = new PolicySummaryService(_requestContext).GetPolicySummary(projectId);
            var policyDetails = GetPolicyDetails(_requestContext.BaseUrl, projectId);

            return SyncPolicy(jiraPolicy, policySummary, policyDetails);
        }

        public JiraPolicy GetJiraPolicy(long projectId)
        {
            var project = new JiraService(_requestContext).GetProject(projectId);

            var policy = new JiraPolicy
            {
                BaseUrl = _baseUrl,
                SharedSecret = _sharedSecret,
                ProjectId = projectId
            };
            var jiraService = new JiraService(context);

            var project = jiraService.GetProject(projectId);

            policy.UserOptions = jiraService.GetUsers(project.Key)
                .Select(user => new User
                {
                    JiraDisplayName = user.displayName,
                    JiraUserKey = user.key
                })
                .ToList();

            return new JiraPolicy
            {
                BaseUrl = _requestContext.BaseUrl,
                SharedSecret = _requestContext.SharedSecret,
                Username = _requestContext.Username,
                Password = _requestContext.Password,
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


