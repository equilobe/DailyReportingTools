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
            policy.AdvancedOptions.NoIndividualDraft = true;
            policy.AdvancedOptions.NoDraft = false;
            policy.GeneratedProperties.IndividualDrafts = null;
            policy.SaveToFile(policyPath);
        }

        public static void SetPolicyFinalReport(Policy policy, string policyPath)
        {
            policy.AdvancedOptions.NoDraft = true;
            policy.SaveToFile(policyPath);
        }

        public static string GetPolicyPath(string key)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var policyPath = basePath + "\\Policies\\" + key + ".xml";
            return policyPath;
        }

        public static Policy CreatePolicy(string id)
        {
            var policyPath = PolicyService.GetPolicyPath(id);
            var policy = Policy.CreateFromFile(policyPath);
            return policy;
        }
    }
}