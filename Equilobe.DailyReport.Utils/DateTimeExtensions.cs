using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        public static long AsUnixTimestampSeconds(this DateTime time)
        {
            return Convert.ToInt64((time - UnixEpoch).TotalSeconds);
        }



        public static DateTime EndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static string CurrentMonth(this DateTime date)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);
        }


        public static DateTime ToOriginalTimeZone(this DateTime date, TimeSpan offsetFromUtc)
        {
            return TimeZoneInfo.ConvertTimeToUtc(date).Add(offsetFromUtc);
        }

    }
}
