using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter.Model
{
    public static class PolicyExtensions
    {
        public static bool CanSendFullDraft(this Policy policy)
        {
            if (policy.AdvancedOptions.NoIndividualDraft)
                return true;

            if (policy.GeneratedProperties.IndividualDrafts == null || policy.GeneratedProperties.IndividualDrafts.Count == 0)
                return false;

            var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
            if (draftsInfo.Exists(i => i.Confirmed == false))
                return false;

            return true;
        }

        public static bool SetIndividualDraftConfirmation(this Policy policy, string key, string policyPath)
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

        public static bool CheckIndividualDraftConfirmation(this Policy policy, string key)
        {
            var draft = policy.GeneratedProperties.IndividualDrafts.Find(dr => dr.UserKey == key);

            return draft.Confirmed;
        }
    }
}
