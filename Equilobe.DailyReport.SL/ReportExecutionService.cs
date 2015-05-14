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
using Equilobe.DailyReport.Models.Jira;
using System.Data.Entity;

namespace Equilobe.DailyReport.SL
{
    public class ReportExecutionService : IReportExecutionService
    {
        public ITaskSchedulerService TaskSchedulerService { get; set; }
        public ITimeZoneService TimeZoneService { get; set; }
        public IDataService DataService { get; set; }

        public SimpleResult SendReport(ExecutionContext context)
        {
            var offsetFromUtc = GetOffsetFromProjectKey(context.Id);

            if (context.Date.Date != DateTime.Now.ToOriginalTimeZone(offsetFromUtc).Date)
                return SimpleResult.Error("Cannot confirm full draft report for another date!");

            if (!CanSendFullDraft(context))
                return SimpleResult.Error("Not all individual draft reports were confirmed!");

            SetFinalDraftConfirmation(context);

            context.Scope = SendScope.SendReport;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Final report successfully sent!");

            return SimpleResult.Error("Error in sending the final report!");
        }

        public SimpleResult SendDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend full draft report for another date!");

            if (!CanSendFullDraft(context) && !IsForcedByLead(context))
                return SimpleResult.Error("Error in resending full draft report!");

            context.Scope = SendScope.SendFinalDraft;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Full draft report successfully resent!");

            return SimpleResult.Error("Error in resending full draft report!");
        }

        public SimpleResult ConfirmIndividualDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot confirm individual draft report for another date!");

            var canConfirm = CanConfirm(context);
            if (canConfirm.HasError)
                return canConfirm;

            if (!MarkIndividualDraftAsConfirmed(context))
                return SimpleResult.Error("Error in confirming individual draft report!");

            if (CanSendFullDraft(context))
            {
                context.Scope = SendScope.SendFinalDraft;
                SetReportExecutionInstance(context);
                if (!TryRunReport(context))
                    return SimpleResult.Error("Individual draft report successfully confirmed. Error in sending full draft report!");
            }

            return SimpleResult.Success("Individual draft report successfully confirmed!");
        }

        public SimpleResult SendIndividualDraft(ExecutionContext context)
        {
            if (context.Date.Date != DateTime.Today)
                return SimpleResult.Error("Cannot resend individual draft report for another date!");

            var result = CanSendIndividualDraft(context);
            if (result.HasError)
                return result;

            context.Scope = SendScope.SendIndividualDraft;
            SetReportExecutionInstance(context);

            if (!TryRunReport(context))
                return SimpleResult.Error("Error in resending individual draft report!");

            return SimpleResult.Success("Individual draft report successfully resent!");
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

        public string GetFullDraftRecipients(AdvancedReportSettings advancedSettings)
        {
            var fullDraftRecipients = new List<string>();

            if (advancedSettings.AdvancedOptions.SendDraftToAllUsers)
                fullDraftRecipients.Add("the entire team");
            else
            {
                if (advancedSettings.AdvancedOptions.SendDraftToProjectManager)
                    fullDraftRecipients.Add("the project lead");
            }

            if (advancedSettings.AdvancedOptions.SendDraftToOthers)
            {
                var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();
                fullDraftRecipients.AddRange(emails);
            }

            return StringExtensions.GetNaturalLanguage(fullDraftRecipients);
        }

        public string GetFinalReportRecipients(AdvancedReportSettings advancedSettings)
        {
            var fullDraftRecipients = new List<string>();

            if (advancedSettings.AdvancedOptions.SendFinalToAllUsers)
                fullDraftRecipients.Add("the entire team");
            else
                fullDraftRecipients.Add("the project lead");

            if (advancedSettings.AdvancedOptions.SendFinalToOthers)
            {
                var emails = advancedSettings.DraftEmails.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                                         .ToList();
                fullDraftRecipients.AddRange(emails);
            }

            return StringExtensions.GetNaturalLanguage(fullDraftRecipients);
        }

        public string GetRemainingUsersToConfirmIndividualDraft(ExecutionContext context, List<JiraUser> jiraUsers)
        {
            var usernamesToConfirm = new List<string>();
            var usersToConfirm = new List<string>();
            var individualDraftConfirmations = new List<IndividualDraftConfirmation>();

            using (var db = new ReportsDb())
            {
                var basicSettingsId = db.IndividualDraftConfirmations.Single(uidc => uidc.UniqueUserKey == context.DraftKey).BasicSettingsId;
                individualDraftConfirmations = db.IndividualDraftConfirmations.ToList();
                usernamesToConfirm = individualDraftConfirmations.Where(idc => idc.BasicSettingsId == basicSettingsId &&
                                                                                  idc.ReportDate == context.Date.DateToString() &&
                                                                                  (idc.LastDateConfirmed == null || 
                                                                                  idc.LastDateConfirmed.Value.Date.Date != DateTime.Today))
                                                                    .Select(qr => qr.Username)
                                                                    .ToList();
            }

            usernamesToConfirm.ForEach(usernameToConfirm =>
            {
                jiraUsers.ForEach(jiraUser =>
                {
                    if (usernameToConfirm == jiraUser.name)
                        usersToConfirm.Add(jiraUser.displayName);
                });
            });

            return StringExtensions.GetNaturalLanguage(usersToConfirm);
        }

        public bool CanSendFullDraft(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.Id);
                var individualReports = report.IndividualDraftConfirmations.Where(dr => dr.ReportDate == context.Date.DateToString()).ToList();

                if (WasFinalReportSent(context, report))
                    return false;

                if (WasFullDraftReportSent(context, report))
                    return true;

                if (report.SerializedAdvancedSettings == null)
                    return false;

                var policy = Deserialization.XmlDeserialize<AdvancedReportSettings>(report.SerializedAdvancedSettings.PolicyString);

                if (report == null || policy.AdvancedOptions.NoDraft)
                    return false;

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return true;

                if (report.IndividualDraftConfirmations == null || report.IndividualDraftConfirmations.Count == 0)
                    return false;

                if (ExistsUnconfirmedDraft(individualReports))
                    return false;

                return true;
            }
        }

        private bool WasFinalReportSent(ExecutionContext context, BasicSettings report)
        {
            return (report.ReportExecutionSummary.LastFinalReportSentDate != null && report.ReportExecutionSummary.LastFinalReportSentDate.Value.Date == context.Date.Date);
        }

        private bool WasFullDraftReportSent(ExecutionContext context, BasicSettings report)
        {
            return (report.ReportExecutionSummary != null && report.ReportExecutionSummary.LastDraftSentDate != null && report.ReportExecutionSummary.LastDraftSentDate.Value.Date == context.Date.Date);
        }

        SimpleResult CanSendIndividualDraft(ExecutionContext context)
        {
            var reportSettings = new BasicSettings();
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.Id);
                report.CopyPropertiesOnObjects(reportSettings);
            }

            if (WasFullDraftReportSent(context, reportSettings))
                return SimpleResult.Error("Cannot resend individual draft if full draft was already sent!");

            if (IsIndividualDraftConfirmed(context, reportSettings.IndividualDraftConfirmations))
                return SimpleResult.Error("Cannot resend individual draft if it's confirmed!");

            return SimpleResult.Success("");
        }

        void SetFinalDraftConfirmation(ExecutionContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.Id);
                if (report.FinalDraftConfirmation == null)
                    report.FinalDraftConfirmation = new FinalDraftConfirmation();
                report.FinalDraftConfirmation.LastFinalDraftConfirmationDate = DateTime.Now;

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

            if (WasFullDraftReportSent(context, basicSettings))
                return SimpleResult.Error("Full draft report already sent!");

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

        public bool IsForcedByLead(ExecutionContext context)
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

        TimeSpan GetOffsetFromProjectKey(string key)
        {
            var timeZoneId = DataService.GetTimeZoneIdFromProjectKey(key);
            return TimeZoneService.GetOffsetFromTimezoneId(timeZoneId);
        }

        #endregion
    }
}
