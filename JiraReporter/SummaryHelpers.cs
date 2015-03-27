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
            while (dateIterator <= endDate)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday && !MonthlyOptionsHelpers.SearchDateInOverrides(currentOverrides, dateIterator))
                    days++;
                dateIterator = dateIterator.AddDays(1);
            }
            return days;
        }

        public static int GetSprintDaysWorked(JiraReport context)
        {
            var now = DateTime.Now.ToOriginalTimeZone(context.OffsetFromUtc);
            if (now <= context.Sprint.EndDate.AddDays(-1).ToOriginalTimeZone(context.OffsetFromUtc))
                return SummaryHelpers.GetWorkingDays(context.Sprint.StartDate.ToOriginalTimeZone(context.OffsetFromUtc), now.AddDays(-1).Date, context.Policy.MonthlyOptions.Months);

            return SummaryHelpers.GetWorkingDays(context.Sprint.StartDate.ToOriginalTimeZone(context.OffsetFromUtc), context.Sprint.EndDate.ToOriginalTimeZone(context.OffsetFromUtc).AddDays(-1), context.Policy.MonthlyOptions.Months);
        }
    }
}
