using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class JiraOffsetHelper
    {
        public static TimeSpan GetOriginalTimeZoneFromDateAtMidnight(DateTime dateAtMidnight)
        {
            var universalDate = dateAtMidnight.ToUniversalTime();

            var referenceDay = universalDate.Date;

            if (referenceDay.AddHours(12) < universalDate)
                referenceDay = referenceDay.AddDays(1);
            // OffsetFromUtc = new DateTimeOffset(dateAtMidnight).Offset;
            return referenceDay - universalDate;
        }
    }
}
