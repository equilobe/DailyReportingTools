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
        public string GetErrorsMessage(string authorName, List<Error> errors, bool hasNotConfirmedError)
        {
            var message = authorName + " has " + errors.Count + NounWithPlural(errors.Count, "error");
            var messagesList = GetMessagesList(errors, hasNotConfirmedError);
            message += "(";

            foreach (var msg in messagesList)
            {
                message += " " + msg;
                if (msg != messagesList.Last())
                    message += ",";
            }

            message += ")";

            return message;
        }

        #region Helpers for GetErrorsMessage() method

        string NounWithPlural(int count, string noun)
        {
            if (count == 1)
                return noun;

            return noun + "s";
        }

        List<string> GetMessagesList(List<Error> errors, bool hasNotConfirmedError)
        {
            var messagesList = new List<string>();

            var noRemainingEstimateErrors = errors.Count(er => er.Type == ErrorType.HasNoRemaining);
            var completedWithEstimateErrors = errors.Count(er => er.Type == ErrorType.HasRemaining);
            var noTimeSpentErrors = errors.Count(er => er.Type == ErrorType.HasNoTimeSpent);

            if (noRemainingEstimateErrors > 0)
                messagesList.Add(noRemainingEstimateErrors + NounWithPlural(noRemainingEstimateErrors, "item") + " with no remaining estimate");

            if (completedWithEstimateErrors > 0)
                messagesList.Add(completedWithEstimateErrors + " completed " + NounWithPlural(completedWithEstimateErrors, "item") + " with remaining estimate ");

            if (noTimeSpentErrors > 0)
                messagesList.Add(noTimeSpentErrors + " completed " + NounWithPlural(noTimeSpentErrors, "item") + " with no work logged");

            if (hasNotConfirmedError)
                messagesList.Add("has not confirmed individual draft");

            return messagesList;
        }

        #endregion
    }
}
