using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportExecution;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Web;
using System;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportExecutionService : IService
    {
        SimpleResult SendReport(ExecutionContext context);
        SimpleResult SendDraft(ExecutionContext context);
        SimpleResult ConfirmIndividualDraft(ExecutionContext context);
        SimpleResult SendIndividualDraft(ExecutionContext context);
        void SaveIndividualDraftConfirmation(UserConfirmationContext context);
        void MarkExecutionInstanceAsExecuted(ItemContext context);
        void AddScheduledExecutionInstance(JiraReport report);
        void MarkExecutionInstanceStatus(ExecutionInstanceContext context);
        SimpleResult CanSendFullDraft(ConfirmationContext context);
        bool IsForcedByLead(ExecutionContext context);
        string GetFullDraftRecipients(AdvancedReportSettings advancedSettings);
        string GetFinalReportRecipients(AdvancedReportSettings advancedSettings);
        string GetRemainingUsersToConfirmIndividualDraft(ConfirmationContext context);
        void MarkSentDates(JiraReport report);
        void UpdateDeprecatedExecutionInstances(ExecutionInstanceContext context);
        //void SetExecutionInstanceUniqueUserKey(long id, string key);
    }
}
