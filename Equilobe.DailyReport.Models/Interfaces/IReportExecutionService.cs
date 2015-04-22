using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportExecution;
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
        bool CanSendFullDraft(ExecutionContext context);
        bool IsForcedByLead(ExecutionContext context);
        string GetFullDraftRecipients(AdvancedReportSettings advancedSettings);
        string GetFinalReportRecipients(AdvancedReportSettings advancedSettings);
        string GetRemainingUsersToConfirmIndividualDraft(ExecutionContext context, List<JiraUser> jiraUsers);
    }
}
