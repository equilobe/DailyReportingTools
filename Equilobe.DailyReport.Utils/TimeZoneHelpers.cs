using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Utils
{
    public class TimeZoneHelpers
    {
        public static TimeSpan GetOffsetFromTimezoneId(string timeZoneId)
        {
            if (timeZoneId == null)
                return new TimeSpan();

            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return timeZoneInfo.GetUtcOffset(DateTime.Now.ToUniversalTime());
        }
    }
}
