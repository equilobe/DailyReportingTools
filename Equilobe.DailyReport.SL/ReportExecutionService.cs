using Equilobe.DailyReport.DAL;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportExecution;
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
    public class ReportExecutionService : IReportExecutionService
    {
        public ITaskSchedulerService TaskSchedulerService { get; set; }

        public SimpleResult SendReport(ExecutionContext context)
        {            
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot confirm report for another date");

            if (!CanSendFullDraft(context))
                return SimpleResult.Error("Not all individual drafts were confirmed");

            SetFinalDraftConfirmation(context);

            context.Scope = SendScope.SendReport;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Report confirmed. Final report sent");
            else
                return SimpleResult.Error("Error in sending the final report");
        }

        public SimpleResult SendDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend draft for another date");

            if (!CanSendFullDraft(context))
                return SimpleResult.Error("Cannot send report if not all individual drafts were confirmed");

            context.Scope = SendScope.SendFinalDraft;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Draft report was resent");
            else
                return SimpleResult.Error("Error in sending draft report");

        }

        public SimpleResult ConfirmIndividualDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot confirm report for another date");

            if (CheckIndividualConfirmation(context))
                return SimpleResult.Error("This draft is already confirmed");

            if (!MarkIndividualDraftAsConfirmed(context))
                return SimpleResult.Error("Error in confirmation");

            if (CanSendFullDraft(context))
            {
                context.Scope = SendScope.SendFinalDraft;
                SetReportExecutionInstance(context);
                if (!TryRunReport(context))
                    return SimpleResult.Error("Report confirmed. Error in sending full draft report");

                return SimpleResult.Error("Report confirmed. Full draft sent");
            }

            return SimpleResult.Success("Report confirmed");
        }

        public SimpleResult SendIndividualDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend report for another date");

            if (CheckIndividualConfirmation(context))
                return SimpleResult.Error("Draft is already confirmed. Can't resend");

            context.Scope = SendScope.SendIndividualDraft;
            SetReportExecutionInstance(context);
            
            if (!TryRunReport(context))
                return SimpleResult.Error("Error in sending individual draft");

            return SimpleResult.Success("Report resent");
        }

        public void SaveIndividualDraftConfirmation(UserConfirmationContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.Id);
                if (report.IndividualDraftConfirmations == null)
                    report.IndividualDraftConfirmations = new List<IndividualDraftConfirmation>();
                var individualDraft = report.IndividualDraftConfirmations.SingleOrDefault(dr => dr.Username == context.Info.Username);
                if (individualDraft != null)
                    UpdateIndividualDraftConfirmation(individualDraft, context.Info);
                else
                    CreateNewIndividualDraftConfirmation(context.Info, report.IndividualDraftConfirmations);

                db.SaveChanges();
            }
        }

        #region Helpers

        bool CanSendFullDraft(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                var individualReports = report.IndividualDraftConfirmations.ToList();
                var isForcedByLead = IsForcedByLead(context.DraftKey, individualReports);

                if (report.SerializedAdvancedSettings == null)
                    return false;

                var policy = Deserialization.XmlDeserialize<AdvancedReportSettings>(report.SerializedAdvancedSettings.PolicyString);

                if (report == null || policy.AdvancedOptions.NoDraft)
                    return false;

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return true;

                if (!isForcedByLead && report.ReportExecutionSummary == null)
                    return false;

                if (isForcedByLead || ( report.ReportExecutionSummary != null && report.ReportExecutionSummary.LastDraftSentDate != null && report.ReportExecutionSummary.LastDraftSentDate.Value.Date == DateTime.Today))
                    return true;

                if (report.IndividualDraftConfirmations == null || report.IndividualDraftConfirmations.Count == 0)
                    return false;

                if (ExistsUnconfirmedDraft(individualReports))
                    return false;

                return true;
            }
        }

        void SetFinalDraftConfirmation(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.Id);
                if (report.FinalDraftConfirmation == null)
                    report.FinalDraftConfirmation = new FinalDraftConfirmation();
                report.FinalDraftConfirmation.LastFinalDraftConfirmationDate = DateTime.Today;

                db.SaveChanges();
            }
        }

        bool MarkIndividualDraftAsConfirmed(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                if (report == null || report.IndividualDraftConfirmations == null)
                    if (VerifyDates(report.ReportExecutionSummary))
                        return false;

                var individualReports = report.IndividualDraftConfirmations.Select(confirmation => confirmation).Where(c => c.BasicSettingsId == report.Id).ToList();
                var draft = individualReports.SingleOrDefault(dr => dr.UniqueUserKey == context.DraftKey);
                if (draft == null)
                    return false;

                draft.LastDateConfirmed = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        bool CheckIndividualConfirmation(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                if (report.IndividualDraftConfirmations == null)
                    return false;

                var draft = report.IndividualDraftConfirmations.SingleOrDefault(d => d.UniqueUserKey == context.DraftKey);
                if (draft == null || draft.LastDateConfirmed == null || draft.LastDateConfirmed.Value.Date != DateTime.Today)
                    return false;

                return true;
            }
        }

        void SetReportExecutionInstance(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.GetBasicSettingsByUniqueProjectKey(context.Id);

                if (report.ReportExecutionInstances == null)
                    report.ReportExecutionInstances = new List<ReportExecutionInstance>();

                report.ReportExecutionInstances.Add(new ReportExecutionInstance
                {
                    DateAdded = DateTime.Now,
                    Scope = context.Scope,
                    UniqueUserKey = context.DraftKey
                });

                db.SaveChanges();
            }
        }

        public void MarkExecutionInstanceAsExecuted(ItemContext context)
        {
            using (var db = new ReportsDb())
            {
                db.SetExecutionInstanceDate(context.Id);
                db.SaveChanges();
            }
        }

        void CreateNewIndividualDraftConfirmation(IndividualDraftInfo draft, ICollection<IndividualDraftConfirmation> confirmations)
        {
            confirmations.Add(new IndividualDraftConfirmation
            {
                IsProjectLead = draft.IsLead,
                UniqueUserKey = draft.UserKey,
                Username = draft.Username
            });
        }

        void UpdateIndividualDraftConfirmation(IndividualDraftConfirmation individualDraft, IndividualDraftInfo draft)
        {
            individualDraft.IsProjectLead = draft.IsLead;
            individualDraft.UniqueUserKey = draft.UserKey;
        }

        bool ExistsUnconfirmedDraft(List<IndividualDraftConfirmation> individualReports)
        {
            return individualReports.Exists(r => r.LastDateConfirmed == null || r.LastDateConfirmed.Value.Date != DateTime.Today);
        }

        bool IsForcedByLead(string userKey, List<IndividualDraftConfirmation> individualConfirmations)
        {
            if (individualConfirmations == null || string.IsNullOrEmpty(userKey))
                return false;

            var user = individualConfirmations.SingleOrDefault(u => u.UniqueUserKey == userKey);
            if (user == null || !user.IsProjectLead)
                return false;

            return true;
        }

        bool VerifyDates(ReportExecutionSummary reportExec)
        {
            return (reportExec == null || (reportExec.LastDraftSentDate != null && reportExec.LastDraftSentDate.Value.Date == DateTime.Today)
                || (reportExec.LastFinalReportSentDate != null && reportExec.LastFinalReportSentDate.Value.Date == DateTime.Today));
        }

        bool TryRunReport(ExecutionContext context)
        {
            return TaskSchedulerService.TryRunReportTask(new Models.TaskScheduling.ProjectContext { UniqueProjectKey = context.Id });
        }

        #endregion
    }
}
