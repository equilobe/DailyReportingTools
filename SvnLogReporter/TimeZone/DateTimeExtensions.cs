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

        static DateTimeExtensions() {
            OffsetFromUtc = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        }

        public static long AsUnixTimestampSeconds(this DateTime time)
        {
            return Convert.ToInt64((time - UnixEpoch).TotalSeconds);
        }

        public static TimeSpan OffsetFromUtc { get; set; }

        public static DateTime ToOriginalTimeZone(this DateTime date)
        {
         //   return date.ToUniversalTime().Add(OffsetFromUtc);
            return TimeZoneInfo.ConvertTimeToUtc(date).Add(OffsetFromUtc);
        }

        public static DateTime ToGithubTime(this DateTime date)
        {
            return date.Add(OffsetFromUtc.Negate());
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

        public static void SetOriginalTimeZoneFromDateAtMidnight(DateTime dateAtMidnight)
        {
            var universalDate = dateAtMidnight.ToUniversalTime();

            var referenceDay = universalDate.Date;

            if(referenceDay.AddHours(12) < universalDate )
                referenceDay = referenceDay.AddDays(1);
           // OffsetFromUtc = new DateTimeOffset(dateAtMidnight).Offset;
            OffsetFromUtc = referenceDay - universalDate;
        }

        public static void SetOriginalTimeZoneFromDateAtMidnight(DateTime dateAtMidnight, DateTime localFromDate)
        {
            var offsetDateNow = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
            OffsetFromUtc = offsetDateNow - (dateAtMidnight - localFromDate);
        }
    }
}
