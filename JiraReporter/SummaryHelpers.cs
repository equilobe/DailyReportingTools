using Equilobe.DailyReport.Models.ReportPolicy;
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
    }
}
