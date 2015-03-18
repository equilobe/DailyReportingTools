using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.ReportExecution;
using System;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IReportExecutionService
    {
        SimpleResult SendReport(ExecutionContext context);
        SimpleResult SendDraft(ExecutionContext context);
        SimpleResult ConfirmIndividualDraft(ExecutionContext context);
        SimpleResult SendIndividualDraft(ExecutionContext context);
        void SaveIndividualDraftConfirmation(UserConfirmationContext context);
        void MarkExecutionInstanceAsExecuted(ItemContext context);
    }
}
