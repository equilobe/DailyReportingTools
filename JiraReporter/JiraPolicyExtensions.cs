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
        public static bool CheckIndividualDraftConfirmation(this JiraPolicy policy, string key)
        {
            //var draft = policy.GeneratedProperties.IndividualDrafts.Find(dr => dr.UserKey == key);

            //return draft.Confirmed;
            return true;
        }
    }
}
