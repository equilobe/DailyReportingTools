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
            var offsetFromUtc = DataService.GetOffsetFromProjectKey(context.Id);

            if (DateTimeHelpers.CompareDayServerWithJira(context.Date, DateTime.Now, offsetFromUtc) != 1)
                return SimpleResult.Error("Cannot confirm full draft report for another date!");

            var confirmationContext = new ConfirmationContext
            {
                ExecutionContext = context,
                OffsetFromUtc = offsetFromUtc
            };

            var canSendFullDraft = CanSendFullDraft(confirmationContext);

            if (canSendFullDraft.HasError)
                return canSendFullDraft;

            SetFinalDraftConfirmation(context);

            context.Scope = SendScope.SendReport;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Final report successfully sent!");

            return SimpleResult.Error("Error in sending the final report!");
        }

        public SimpleResult SendDraft(ExecutionContext context)
        {
            var offsetFromUtc = DataService.GetOffsetFromProjectKey(context.Id);

            if (DateTimeHelpers.CompareDayServerWithJira(context.Date, DateTime.Now, offsetFromUtc) != 1)
                return SimpleResult.Error("Cannot resend full draft report for another date!");

            var confirmationContext = new ConfirmationContext
            {
                ExecutionContext = context,
                OffsetFromUtc = offsetFromUtc
            };

            var canSendFullDraft = CanSendFullDraft(confirmationContext);

            if (canSendFullDraft.HasError && !IsForcedByLead(context))
                return canSendFullDraft;

            context.Scope = SendScope.SendFinalDraft;
            SetReportExecutionInstance(context);

            if (TryRunReport(context))
                return SimpleResult.Success("Full draft report successfully resent!");

            return SimpleResult.Error("Error in resending full draft report!");
        }

        public SimpleResult ConfirmIndividualDraft(ExecutionContext context)
        {
            var offsetFromUtc = DataService.GetOffsetFromProjectKey(context.Id);

            if (DateTimeHelpers.CompareDayServerWithJira(context.Date, DateTime.Now, offsetFromUtc) != 1)
                return SimpleResult.Error("Cannot confirm individual draft report for another date!");

            var confirmationContext = new ConfirmationContext
            {
                ExecutionContext = context,
                OffsetFromUtc = offsetFromUtc
            };

            var confirm = ConfirmIndividualDraft(confirmationContext);

            if (confirm.HasError)
                return confirm;

            if (!CanSendFullDraft(confirmationContext).HasError)
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
            var offsetFromUtc = DataService.GetOffsetFromProjectKey(context.Id);

            if (DateTimeHelpers.CompareDayServerWithJira(context.Date, DateTime.Now, offsetFromUtc) != 1)
                return SimpleResult.Error("Cannot resend individual draft report for another date!");

            var confirmationContext = new ConfirmationContext
            {
                ExecutionContext = context,
                OffsetFromUtc = offsetFromUtc
            };

            var result = CanSendIndividualDraft(confirmationContext);
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

        public string GetRemainingUsersToConfirmIndividualDraft(ConfirmationContext context)
        {
            var usernamesToConfirm = new List<string>();
            var usersToConfirm = new List<string>();
            var individualDraftConfirmations = new List<IndividualDraftConfirmation>();

            using (var db = new ReportsDb())
            {
                var basicSettingsId = db.IndividualDraftConfirmations.Single(uidc => uidc.UniqueUserKey == context.ExecutionContext.DraftKey).BasicSettingsId;
                individualDraftConfirmations = db.IndividualDraftConfirmations.ToList();
                usernamesToConfirm = individualDraftConfirmations.Where(idc => idc.BasicSettingsId == basicSettingsId &&
                                                                                  idc.ReportDate == context.ExecutionContext.Date.DateToString() &&
                                                                                  DateTimeHelpers.CompareDayServerWithJira(context.ExecutionContext.Date, idc.LastDateConfirmed, context.OffsetFromUtc) != 1)
                                                                    .Select(qr => qr.Username)
                                                                    .ToList();
            }

            usernamesToConfirm.ForEach(usernameToConfirm =>
            {
                context.Users.ForEach(jiraUser =>
                {
                    if (usernameToConfirm == jiraUser.name)
                        usersToConfirm.Add(jiraUser.displayName);
                });
            });

            return StringExtensions.GetNaturalLanguage(usersToConfirm);
        }

        public SimpleResult CanSendFullDraft(ConfirmationContext context)
        {
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == context.ExecutionContext.Id);
                var individualReports = report.IndividualDraftConfirmations.Where(dr => dr.ReportDate == context.ExecutionContext.Date.DateToString()).ToList();

                if (WasFinalReportSent(report.ReportExecutionSummary, context.OffsetFromUtc))
                    return SimpleResult.Error("Final report was already sent");

                if (WasFullDraftReportSent(report.ReportExecutionSummary, context.OffsetFromUtc))
                    return SimpleResult.Success("");

                if (report.SerializedAdvancedSettings == null)
                    return SimpleResult.Error("Report settings are missing");

                var policy = Deserialization.XmlDeserialize<AdvancedReportSettings>(report.SerializedAdvancedSettings.PolicyString);

                if (report == null)
                    return SimpleResult.Error("Project not found");

                if (policy.AdvancedOptions.NoDraft)
                    return SimpleResult.Error("Project is not configured to send draft");

                if (policy.AdvancedOptions.NoIndividualDraft)
                    return SimpleResult.Success("");

                if (report.IndividualDraftConfirmations.IsEmpty())
                    return SimpleResult.Error("Individual reports must be sent and confirmed first");

                if (ExistsUnconfirmedDraft(individualReports, context.OffsetFromUtc))
                    return SimpleResult.Error("Not all individual drafts were confirmed!");

                return SimpleResult.Success("");
            }
        }

        bool WasFinalReportSent(ReportExecutionSummary reportExecSummary, TimeSpan offsetFromUtc)
        {
            if (reportExecSummary == null)
                return false;

            return (DateTimeHelpers.CompareDay(reportExecSummary.LastFinalReportSentDate, DateTime.Now, offsetFromUtc) == 1);
        }

        bool WasFullDraftReportSent(ReportExecutionSummary reportExecSummary, TimeSpan offsetFromUtc)
        {
            if (reportExecSummary == null)
                return false;

            return (DateTimeHelpers.CompareDay(reportExecSummary.LastDraftSentDate, DateTime.Now, offsetFromUtc) == 1);
        }

        SimpleResult CanSendIndividualDraft(ConfirmationContext context)
        {
            var reportSettings = new BasicSettings();
            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(r => r.UniqueProjectKey == context.ExecutionContext.Id);
                report.CopyPropertiesOnObjects(reportSettings);
            }

            context.IndividualDrafts = reportSettings.IndividualDraftConfirmations.ToList();
            var draft = context.IndividualDrafts.SingleOrDefault(d => d.UniqueUserKey == context.ExecutionContext.DraftKey);

            if (WasFullDraftReportSent(reportSettings.ReportExecutionSummary, context.OffsetFromUtc))
                return SimpleResult.Error("Cannot resend individual draft if full draft was already sent!");

            if (draft == null)
                return SimpleResult.Error("Error in confirmation. Draft was not found");

            if (IsIndividualDraftConfirmed(draft, context.OffsetFromUtc))
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

        void MarkIndividualDraftAsConfirmed(string draftKey)
        {
            using (var db = new ReportsDb())
            {
                var draft = db.IndividualDraftConfirmations.Single(dr => dr.UniqueUserKey == draftKey);
                draft.LastDateConfirmed = DateTime.Now;
                db.SaveChanges();
            }
        }

        SimpleResult ConfirmIndividualDraft(ConfirmationContext confirmationContext)
        {
            var basicSettings = new BasicSettings();

            using (var db = new ReportsDb())
            {
                var report = db.BasicSettings.SingleOrDefault(qr => qr.UniqueProjectKey == confirmationContext.ExecutionContext.Id);
                report.CopyPropertiesOnObjects(basicSettings);
            }

            confirmationContext.IndividualDrafts = basicSettings.IndividualDraftConfirmations.ToList();
            var draft = confirmationContext.IndividualDrafts.SingleOrDefault(d => d.UniqueUserKey == confirmationContext.ExecutionContext.DraftKey);

            var canConfirm = CanConfirm(confirmationContext, basicSettings.ReportExecutionSummary, draft);
            if (canConfirm.HasError)
                return canConfirm;

            MarkIndividualDraftAsConfirmed(confirmationContext.ExecutionContext.DraftKey);
            return SimpleResult.Success("Confirmed");
        }

        SimpleResult CanConfirm(ConfirmationContext confirmationContext, ReportExecutionSummary execSummary, IndividualDraftConfirmation draft)
        {
            if (WasFullDraftReportSent(execSummary, confirmationContext.OffsetFromUtc))
                return SimpleResult.Error("Full draft report already sent!");

            if (WasFinalReportSent(execSummary, confirmationContext.OffsetFromUtc))
                return SimpleResult.Error("Final report already sent");

            if (confirmationContext.IndividualDrafts.IsEmpty())
                return SimpleResult.Error("Cannot confirm individual draft");

            if (draft == null)
                return SimpleResult.Error("Error in confirmation. Draft was not found");

            if (IsIndividualDraftConfirmed(draft, confirmationContext.OffsetFromUtc))
                return SimpleResult.Error("Individual draft report already confirmed!");

            return SimpleResult.Success("");
        }

        bool IsIndividualDraftConfirmed(IndividualDraftConfirmation draft, TimeSpan offset)
        {
            if (draft != null && draft.LastDateConfirmed != null && draft.LastDateConfirmed.Value.ToOriginalTimeZone(offset).DateToString() == draft.ReportDate)
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

        bool ExistsUnconfirmedDraft(List<IndividualDraftConfirmation> individualReports, TimeSpan offsetFromUtc)
        {
            return individualReports.Exists(r => r.LastDateConfirmed == null || r.LastDateConfirmed.Value.ToOriginalTimeZone(offsetFromUtc).Date.DateToString() != r.ReportDate);
        }

        public bool IsForcedByLead(ExecutionContext context)
        {
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

        bool TryRunReport(ExecutionContext context)
        {
            return TaskSchedulerService.TryRunReportTask(new Models.TaskScheduling.ProjectContext { UniqueProjectKey = context.Id });
        }

        #endregion
    }
}
