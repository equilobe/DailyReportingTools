using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class SummaryHelpers
    {

        public static int GetWorkingDays(DateTime startDate, DateTime endDate, List<Month> currentOverrides)
        {
            DateTime dateIterator = startDate.Date;
            int days = 0;
            while (dateIterator < endDate.Date)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday && !MonthlyOptionsHelpers.SearchDateInOverrides(currentOverrides, dateIterator))
                    days++;
                dateIterator = dateIterator.AddDays(1);
            }
            return days;
        }

        public static int GetSprintDaysWorked(JiraReport context)
        {
            var reportDate = context.ToDate.AddDays(-1);
            if (reportDate <= context.Sprint.EndDate.ToOriginalTimeZone(context.OffsetFromUtc))
                return GetWorkingDays(context.Sprint.StartDate.ToOriginalTimeZone(context.OffsetFromUtc), reportDate.AddDays(1), context.Policy.MonthlyOptions);

            return GetWorkingDays(context.Sprint.StartDate.ToOriginalTimeZone(context.OffsetFromUtc), context.Sprint.EndDate.ToOriginalTimeZone(context.OffsetFromUtc).AddDays(1), context.Policy.MonthlyOptions);
        }
    }
}
