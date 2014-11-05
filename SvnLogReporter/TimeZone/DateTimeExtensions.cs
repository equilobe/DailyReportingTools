using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeExtensions
    {

        static DateTimeExtensions() {
            OffsetFromUtc = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

       
        public static TimeSpan OffsetFromUtc { get; set; }

        public static DateTime ToOriginalTimeZone(this DateTime date)
        {
         //   return date.ToUniversalTime().Add(OffsetFromUtc);
            return TimeZoneInfo.ConvertTimeToUtc(date).Add(OffsetFromUtc);
        }

        public static DateTime GetEndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }


        public static void SetOriginalTimeZoneFromDateAtMidnight(DateTime dateAtMidnight)
        {
            var universalDate = dateAtMidnight.ToUniversalTime();

            var referenceDay = universalDate.Date;

            if(referenceDay.AddHours(12) < universalDate )
                referenceDay = referenceDay.AddDays(1);
           // OffsetFromUtc = new DateTimeOffset(dateAtMidnight).Offset;
            OffsetFromUtc = referenceDay - universalDate;
        }
    }
}
