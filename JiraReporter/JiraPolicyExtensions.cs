using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class PolicyExtensions
    {
        //TODO : adjust method to work with db
        public static bool CanSendFullDraft(this JiraPolicy policy, string draftKey="")
        {
            //if (policy.IsForcedByLead(draftKey) || policy.GeneratedProperties.WasForcedByLead)
            //    return true;

            //if (policy.AdvancedOptions.NoIndividualDraft)
            //    return true;

            //if (policy.GeneratedProperties.IndividualDrafts == null || policy.GeneratedProperties.IndividualDrafts.Count == 0)
            //    return false;

            //var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
            //if (draftsInfo.Exists(i => i.Confirmed == false))
            //    return false;

            return true;
        }
        //TODO : adjust method to work with db
        public static bool IsForcedByLead(this JiraPolicy policy, string draftKey)
        {
            //if (policy.GeneratedProperties.IndividualDrafts == null)
            //    return false;

            //var draftsInfo = policy.GeneratedProperties.IndividualDrafts;
            //var draft = draftsInfo.Find(dr => dr.UserKey == draftKey);
            //if (draft == null || !draft.IsLead)
           //     return false;

            return true;
        }

        //TODO : adjust method to work with db
        public static bool CheckIndividualDraftConfirmation(this JiraPolicy policy, string key)
        {
            //var draft = policy.GeneratedProperties.IndividualDrafts.Find(dr => dr.UserKey == key);

            //return draft.Confirmed;
            return true;
        }
    }
}
