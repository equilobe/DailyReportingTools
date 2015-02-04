using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Helpers
{
    public class PolicyService
    {
        public static void SetPolicyFullDraft(Policy policy, string policyPath)
        {
            policy.GeneratedProperties.IsFinalDraftConfirmed = false;
            policy.SaveToFile(policyPath);
        }

        public static void SetPolicyFinalReport(Policy policy, string policyPath)
        {
            policy.GeneratedProperties.IsFinalDraftConfirmed = true;
            policy.SaveToFile(policyPath);
        }

        public static string GetPolicyPath(string key)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var policyPath = basePath + @"\Policies\" + key + ".xml";
            return policyPath;
        }

        public static Policy LoadPolicy(string id)
        {
            var policyPath = PolicyService.GetPolicyPath(id);
            var policy = Policy.LoadFromFile(policyPath);
            return policy;
        }
    }
}