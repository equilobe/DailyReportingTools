using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Web;
using Equilobe.DailyReport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class ReportService
    {
        public static bool CanSendFullDraft(string uniqueProjectKey, string userKey = "")
        {
            using(var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr=>qr.UniqueProjectKey == uniqueProjectKey);
                var individualReports = report.IndividualDraftConfirmations.Select(confirmation => confirmation).Where(c => c.ReportSettingsId == report.Id).ToList();

                if (report.SerializedPolicy == null)
                    return false;

                var policy = Deserialization.XmlDeserialize<PolicyDetails>(report.SerializedPolicy.PolicyString);

                if(report == null)
                    return false;

                if (IsForcedByLead(userKey, individualReports) || report.ReportExecutionSummary.LastDraftSentDate == DateTime.Today)
                    return true;

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return true;

                if (report.IndividualDraftConfirmations == null || report.IndividualDraftConfirmations.Count == 0)
                    return false;

                if(individualReports.Exists(r=>r.LastDateConfirmed != DateTime.Today))
                    return false;

                return true;
            }
           
        }

        private static bool IsForcedByLead(string userKey, List<IndividualDraftConfirmation> individualConfirmations)
        {
            if (individualConfirmations == null)
                return false;

            var user = individualConfirmations.SingleOrDefault(u => u.UniqueUserKey == userKey);
            if (user == null || !user.IsProjectLead)
                return false;

            return true;
        }

        public static void SetFinalDraftConfirmation(string key)
        {
            using(var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == key);
                report.FinalDraftConfirmation.LastFinalDraftConfirmationDate = DateTime.Today;

                db.SaveChanges();
            }
        }

        public static bool ConfirmIndividualDraft(string uniqueProjectKey, string draftKey)
        {
            using(var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == uniqueProjectKey);
                if (report == null || report.IndividualDraftConfirmations == null)
                    return false;

                var individualReports = report.IndividualDraftConfirmations.Select(confirmation => confirmation).Where(c => c.ReportSettingsId == report.Id).ToList();
                var draft = individualReports.SingleOrDefault(dr => dr.UniqueUserKey == draftKey);
                if (draft == null)
                    return false;

                draft.LastDateConfirmed = DateTime.Today;
                db.SaveChanges();
                return true;
            }
        }

        public static bool CheckIndividualConfirmation(string uniqueProjectKey, string draftKey)
        {
            using(var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == uniqueProjectKey);
                if (report.IndividualDraftConfirmations == null)
                    return false;

                var draft = report.IndividualDraftConfirmations.SingleOrDefault(d => d.UniqueUserKey == draftKey);
                if (draft == null || draft.LastDateConfirmed != DateTime.Today)
                    return false;

                return true;
            }
        }

        public static void SaveIndividualDraftConfirmation(string uniqueProjectKey, IndividualDraftInfo draft)
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r=>r.UniqueProjectKey == uniqueProjectKey);
                if(report.IndividualDraftConfirmations == null)
                    report.IndividualDraftConfirmations = new List<IndividualDraftConfirmation>();
                var individualDraft = report.IndividualDraftConfirmations.SingleOrDefault(dr=>dr.Username == draft.Username);
                if (individualDraft != null)
                    UpdateIndividualDraftConfirmation(individualDraft, draft);
                else
                    CreateNewIndividualDraftConfirmation(draft, report.IndividualDraftConfirmations);

                db.SaveChanges();
            }
        }

        private static void CreateNewIndividualDraftConfirmation(IndividualDraftInfo draft, ICollection<IndividualDraftConfirmation> confirmations)
        {
            confirmations.Add(new IndividualDraftConfirmation
            {
                IsProjectLead = draft.IsLead,
                UniqueUserKey = draft.UserKey,
                Username = draft.Username
            });
        }

        private static void UpdateIndividualDraftConfirmation(IndividualDraftConfirmation individualDraft, IndividualDraftInfo draft)
        {
            individualDraft.IsProjectLead = draft.IsLead;
            individualDraft.UniqueUserKey = draft.UserKey;
        }
    }
}
