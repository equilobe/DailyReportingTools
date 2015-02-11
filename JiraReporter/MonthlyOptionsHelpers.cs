using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    static class MonthlyOptionsHelpers
    {
        public static List<int> GetNonWorkingDays(Month month)
        {
            var nonWorkingDays = new List<int>();
            var daysString = month.NonWorkingDays.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var number in daysString)
                nonWorkingDays.Add(Int32.Parse(number));
            return nonWorkingDays;
        }

        public static Month SearchOverride(List<Month> overrides, DateTime day)
        {
            if (overrides == null)
                return null;

            return (overrides.Find(o => o.MonthName.ToLower() == day.CurrentMonth().ToLower()));
        }

        public static bool SearchDateInOverrides(List<Month> overrides, DateTime date)
        {
            var currentOverride = new Month();
            if (overrides != null)
                currentOverride = SearchOverride(overrides, date);
            if (currentOverride == null || currentOverride.NonWorkingDaysList == null)
                return false;
            else
                return currentOverride.NonWorkingDaysList.Exists(d => d == date.Day);
        }
    }
}
