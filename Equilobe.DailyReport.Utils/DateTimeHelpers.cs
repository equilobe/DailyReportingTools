using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Utils
{
    public class DateTimeHelpers
    {
        public static int CompareDay(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue || !date2.HasValue)
                return -1;

            if (date1.Value.Date == date2.Value.Date)
                return 1;

            return 0;
        }

        public static int CompareDay(DateTime? date1, DateTime? date2, TimeSpan offsetFromUtc)
        {
            if (!date1.HasValue || !date2.HasValue)
                return -1;


            if (date1.Value.ToOriginalTimeZone(offsetFromUtc).Date == date2.Value.ToOriginalTimeZone(offsetFromUtc).Date)
                return 1;

            return 0;
        }

        public static int CompareDayServerWithJira(DateTime? serverDate, DateTime? jiraDate, TimeSpan offsetFromUtc)
        {
            if (!serverDate.HasValue || !jiraDate.HasValue)
                return -1;

            if (serverDate.Value.Date == jiraDate.Value.ToOriginalTimeZone(offsetFromUtc).Date)
                return 1;

            return 0;
        }
    }
}
