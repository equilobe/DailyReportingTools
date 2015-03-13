using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
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
        public string _uniqueProjectKey { get; set; }
        public JiraReport _report { get; set; }

        public ReportService()
        {

        }

        public ReportService(string uniqueProjectKey)
        {
            _uniqueProjectKey = uniqueProjectKey;
        }

        public ReportService(JiraReport report)
        {
            _report = report;
        }

        public bool CanSendFullDraft(string userKey = "")
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == _uniqueProjectKey);
                var individualReports = report.IndividualDraftConfirmations.Select(confirmation => confirmation).Where(c => c.ReportSettingsId == report.Id).ToList();
                var isForcedByLead = IsForcedByLead(userKey, individualReports);

                if (report.SerializedPolicy == null)
                    return false;

                var policy = Deserialization.XmlDeserialize<PolicyDetails>(report.SerializedPolicy.PolicyString);

                if (report == null || policy.AdvancedOptions.NoDraft)
                    return false;

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return true;

                if (!isForcedByLead && report.ReportExecutionSummary == null)
                    return false;

                if (isForcedByLead || ( report.ReportExecutionSummary != null && report.ReportExecutionSummary.LastDraftSentDate.Value.Date == DateTime.Today))
                    return true;

                if (report.IndividualDraftConfirmations == null || report.IndividualDraftConfirmations.Count == 0)
                    return false;

                if (individualReports.Exists(r => r.LastDateConfirmed.Value.Date != DateTime.Today))
                    return false;

                return true;
            }
        }

        private bool IsForcedByLead(string userKey, List<IndividualDraftConfirmation> individualConfirmations)
        {
            if (individualConfirmations == null)
                return false;

            var user = individualConfirmations.SingleOrDefault(u => u.UniqueUserKey == userKey);
            if (user == null || !user.IsProjectLead)
                return false;

            return true;
        }

        public void SetFinalDraftConfirmation()
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == _uniqueProjectKey);
                if (report.FinalDraftConfirmation == null)
                    report.FinalDraftConfirmation = new FinalDraftConfirmation();
                report.FinalDraftConfirmation.LastFinalDraftConfirmationDate = DateTime.Today;

                db.SaveChanges();
            }
        }

        public bool ConfirmIndividualDraft(string draftKey)
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == _uniqueProjectKey);
                if (report == null || report.IndividualDraftConfirmations == null)
                    if (VerifyDates(report.ReportExecutionSummary))
                        return false;

                var individualReports = report.IndividualDraftConfirmations.Select(confirmation => confirmation).Where(c => c.ReportSettingsId == report.Id).ToList();
                var draft = individualReports.SingleOrDefault(dr => dr.UniqueUserKey == draftKey);
                if (draft == null)
                    return false;

                draft.LastDateConfirmed = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        private static bool VerifyDates(ReportExecutionSummary reportExec)
        {
            return (reportExec == null || (reportExec.LastDraftSentDate != null && reportExec.LastDraftSentDate.Value.Date == DateTime.Today)
                || (reportExec.LastFinalReportSentDate != null && reportExec.LastFinalReportSentDate.Value.Date == DateTime.Today));           
        }

        public bool CheckIndividualConfirmation(string draftKey)
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(qr => qr.UniqueProjectKey == _uniqueProjectKey);
                if (report.IndividualDraftConfirmations == null)
                    return false;

                var draft = report.IndividualDraftConfirmations.SingleOrDefault(d => d.UniqueUserKey == draftKey);
                if (draft == null || draft.LastDateConfirmed == null || draft.LastDateConfirmed.Value.Date != DateTime.Today)
                    return false;

                return true;
            }
        }

        public void SaveIndividualDraftConfirmation(IndividualDraftInfo draft)
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == _uniqueProjectKey);
                if (report.IndividualDraftConfirmations == null)
                    report.IndividualDraftConfirmations = new List<IndividualDraftConfirmation>();
                var individualDraft = report.IndividualDraftConfirmations.SingleOrDefault(dr => dr.Username == draft.Username);
                if (individualDraft != null)
                    UpdateIndividualDraftConfirmation(individualDraft, draft);
                else
                    CreateNewIndividualDraftConfirmation(draft, report.IndividualDraftConfirmations);

                db.SaveChanges();
            }
        }

        private void CreateNewIndividualDraftConfirmation(IndividualDraftInfo draft, ICollection<IndividualDraftConfirmation> confirmations)
        {
            confirmations.Add(new IndividualDraftConfirmation
            {
                IsProjectLead = draft.IsLead,
                UniqueUserKey = draft.UserKey,
                Username = draft.Username
            });
        }

        private void UpdateIndividualDraftConfirmation(IndividualDraftConfirmation individualDraft, IndividualDraftInfo draft)
        {
            individualDraft.IsProjectLead = draft.IsLead;
            individualDraft.UniqueUserKey = draft.UserKey;
        }

        public void SetReportFromDb()
        {
            using (var db = new ReportsDb())
            {
                var reportSettings = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == _report.UniqueProjectKey);
                _report.Settings = new ReportSettings();
                reportSettings.CopyProperties(_report.Settings);
                if (reportSettings.ReportExecutionSummary != null)
                {
                    if (reportSettings.ReportExecutionSummary.LastDraftSentDate != null)
                        _report.LastDraftSentDate = reportSettings.ReportExecutionSummary.LastDraftSentDate.Value;
                    if (reportSettings.ReportExecutionSummary.LastFinalReportSentDate != null)
                        _report.LastReportSentDate = reportSettings.ReportExecutionSummary.LastFinalReportSentDate.Value;
                }

                if (reportSettings.FinalDraftConfirmation != null)
                    if (reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate != null)
                        _report.LastFinalDraftConfirmationDate = reportSettings.FinalDraftConfirmation.LastFinalDraftConfirmationDate.Value;
            }
        }

        public void SetReportExecutionInstance(SendScope scope, string userKey = "")
        {
            using (var db = new ReportsDb())
            {
                var report = db.ReportSettings.SingleOrDefault(r => r.UniqueProjectKey == _uniqueProjectKey);

                if (report.ReportExecutionInstances == null)
                    report.ReportExecutionInstances = new List<ReportExecutionInstance>();

                report.ReportExecutionInstances.Add(new ReportExecutionInstance
                {
                    DateAdded = DateTime.Now,
                    Scope = scope,
                    UniqueUserKey = userKey
                });

                db.SaveChanges();
            }
        }

        public ReportExecutionInstance GetUnexecutedInstance(ReportSettings report)
        {
            var execInstance = report.ReportExecutionInstances.Where(qr => qr.DateExecuted == null).FirstOrDefault();
            return execInstance;
        }

        public void SetExecutionInstance()
        {
            var unexecutedInstance = GetUnexecutedInstance(_report.Settings);
            if (unexecutedInstance != null)
            {
                _report.ExecutionInstance = new ExecutionInstance();
                unexecutedInstance.CopyProperties(_report.ExecutionInstance);
                using (var db = new ReportsDb())
                {
                    db.SetExecutionDate(unexecutedInstance.Id);
                }
            }
            else
                _report.IsOnSchedule = true;
        }
    }
}
