using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Win32.TaskScheduler;
using SourceControlLogReporter.Model;
using System.Configuration;

namespace DailyReportWeb.Helpers
{
    public class ReportRunner
    {
        public static bool TryRunReport(string id)
        {
            try
            {
                RunReport(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RunReport(string id)
        {
            var key = "DRT-" + id;
            using (var ts = new TaskService())
            {
                var task = ts.AllTasks.Single(t => t.Name == key);
                task.Run();
            }
        }

        public static bool IndividualReportConfirmedByAll(Policy policy)
        {
            var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
            if (draftsInfo.Exists(i => i.Confirmed == false))
                return false;

            return true;
        }

        public static bool SetIndividualDraftConfirmation(string key, Policy policy, string policyPath)
        {
            try
            {
                var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
                var draft = draftsInfo.Find(d => d.UserKey == key);
                draft.Confirmed = true;
                policy.SaveToFile(policyPath);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

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

        public static bool CheckConfirmed(string key, Policy policy)
        {
            {
                var draft = policy.GeneratedProperties.IndividualDrafts.Find(dr => dr.UserKey == key);

                return draft.Confirmed;
            }
        }
    }
}