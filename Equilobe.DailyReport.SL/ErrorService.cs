using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.ReportFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.SL
{
    public class ErrorService : IErrorService
    {
        public string GetMessagesHeader(ErrorContext context)
        {            
            var message ="";

            if (!string.IsNullOrEmpty(context.Assignee))
                message = context.Assignee + " has ";
            else
                message = "Unassigned issues with ";

            message += context.Errors.Count + " " + NounWithPlural(context.Errors.Count, "error");

            return message;
        }

        public List<string> GetMessagesList(ErrorContext context)
        {
            var messagesList = new List<string>();

            var noRemainingEstimateErrors = context.Errors.Count(er => er.Type == ErrorType.HasNoRemaining);
            var completedWithEstimateErrors = context.Errors.Count(er => er.Type == ErrorType.HasRemaining);
            var noTimeSpentErrors = context.Errors.Count(er => er.Type == ErrorType.HasNoTimeSpent);
            bool hasNotConfirmedError = context.Errors.Exists(er => er.Type == ErrorType.NotConfirmed);

            if (noRemainingEstimateErrors > 0)
                messagesList.Add(noRemainingEstimateErrors + " " + NounWithPlural(noRemainingEstimateErrors, "item") + " with no remaining estimate");

            if (completedWithEstimateErrors > 0)
                messagesList.Add(completedWithEstimateErrors + " completed " + NounWithPlural(completedWithEstimateErrors, "item") + " with remaining estimate ");

            if (noTimeSpentErrors > 0)
                messagesList.Add(noTimeSpentErrors + " completed " + NounWithPlural(noTimeSpentErrors, "item") + " with no work logged");

            if (hasNotConfirmedError)
                messagesList.Add("has not confirmed individual draft");

            return messagesList;
        }


        #region Helpers for GetErrorsMessage() method

        string NounWithPlural(int count, string noun)
        {
            if (count == 1)
                return noun;

            return noun + "s";
        }

        #endregion
    }
}
