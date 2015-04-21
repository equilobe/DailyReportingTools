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
using System.Collections;

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
                return SimpleResult.Success("Full draft report successfully confirmed!");
            else
                return SimpleResult.Error("Error in sending the final report");
        }

        public SimpleResult SendDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend draft for another date");

            if (!CanSendFullDraft(context) && !IsForcedByLead(context))
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
                return SimpleResult.Error("Cannot confirm report for another date!");

            var canConfirm = CanConfirm(context);
            if (canConfirm.HasError)
                return canConfirm;

            if (!MarkIndividualDraftAsConfirmed(context))
                return SimpleResult.Error("Error in confirmation");

            if (CanSendFullDraft(context))
            {
                context.Scope = SendScope.SendFinalDraft;
                SetReportExecutionInstance(context);
                if (!TryRunReport(context))
                    return SimpleResult.Error("Report confirmed. Error in sending full draft report");

                return SimpleResult.Success("Individual draft report successfully confirmed!");
            }

            return SimpleResult.Success("Individual draft report successfully confirmed!");
        }

        public SimpleResult SendIndividualDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend report for another date");

            var result = CanSendIndividualDraft(context);
            if (result.HasError)
                return result;

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

        public bool CanSendFullDraft(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                var individualReports = report.IndividualDraftConfirmations.Where(dr => dr.ReportDate.Value == context.Date.Date).ToList();

                if (report.SerializedAdvancedSettings == null)
                    return false;

                var policy = Deserialization.XmlDeserialize<AdvancedReportSettings>(report.SerializedAdvancedSettings.PolicyString);

                if (report == null || policy.AdvancedOptions.NoDraft)
                    return false;

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return true;

                if (report.ReportExecutionSummary != null && report.ReportExecutionSummary.LastDraftSentDate != null && report.ReportExecutionSummary.LastDraftSentDate.Value.Date == DateTime.Today)
                    return true;

                if (report.IndividualDraftConfirmations == null || report.IndividualDraftConfirmations.Count == 0)
                    return false;

                if (ExistsUnconfirmedDraft(individualReports))
                    return false;

                return true;
            }
        }

        SimpleResult CanSendIndividualDraft(ExecutionContext context)
        {
            var reportSettings = new BasicSettings();
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.Id);
                report.CopyPropertiesOnObjects(reportSettings);
            }

            if (WasFinalDraftSentToday(reportSettings.ReportExecutionSummary, context.Date.Date))
                return SimpleResult.Error("Can't resend individual draft if final draft was already sent");

            if (IsIndividualDraftConfirmed(context, reportSettings.IndividualDraftConfirmations))
                return SimpleResult.Error("Can't resend draft after confirmation");

            return SimpleResult.Success("");
        }

        bool WasFinalDraftSentToday(ReportExecutionSummary summary, DateTime reportDate)
        {
            if (summary != null)
                if (summary.LastDraftSentDate != null && summary.LastDraftSentDate.Value.Date == reportDate)
                    return true;

            return false;
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

        SimpleResult CanConfirm(ExecutionContext context)
        {
            var basicSettings = new BasicSettings();

            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                report.CopyPropertiesOnObjects(basicSettings);
            }

            if (WasFinalDraftSentToday(basicSettings.ReportExecutionSummary, context.Date.Date))
                return SimpleResult.Error("Individual draft report already confirmed!");

            if (basicSettings.IndividualDraftConfirmations == null)
                return SimpleResult.Success("Can confirm");

            if (IsIndividualDraftConfirmed(context, basicSettings.IndividualDraftConfirmations))
                return SimpleResult.Error("Individual draft report already confirmed!");

            return SimpleResult.Success("Can confirm");
        }

        bool IsIndividualDraftConfirmed(ExecutionContext context, ICollection<IndividualDraftConfirmation> individualDrafts)
        {
            var draft = individualDrafts.SingleOrDefault(d => d.UniqueUserKey == context.DraftKey);
            if (draft != null && draft.LastDateConfirmed != null && draft.LastDateConfirmed.Value.Date == context.Date.Date)
                return true;

            return false;
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
                IsProjectLead = draft.IsProjectLead,
                UniqueUserKey = draft.UniqueUserKey,
                Username = draft.Username,
                ReportDate = draft.ReportDate
            });
        }

        void UpdateIndividualDraftConfirmation(IndividualDraftConfirmation individualDraft, IndividualDraftInfo draft)
        {
            draft.CopyPropertiesOnObjects(individualDraft);
        }

        bool ExistsUnconfirmedDraft(List<IndividualDraftConfirmation> individualReports)
        {
            return individualReports.Exists(r => r.LastDateConfirmed == null || r.LastDateConfirmed.Value.Date != DateTime.Today);
        }

        bool IsForcedByLead(ExecutionContext context)
        {
            var individualDrafts = new List<IndividualDraftConfirmation>();

            using (var db = new ReportsDb())
            {
                var settings = db.BasicSettings.SingleOrDefault(bs => bs.UniqueProjectKey == context.Id);
                return FindLead(context.DraftKey, settings.IndividualDraftConfirmations);
            }
        }

        bool FindLead(string userKey, ICollection<IndividualDraftConfirmation> individualConfirmations)
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
