using System.Globalization;

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

        public static DateTime ToServerTimeZone(this DateTime date, TimeSpan offsetFromUtc)
        {
            var serverOffset = TimeZoneInfo.Local.BaseUtcOffset;

            return date.Add(offsetFromUtc.Negate()).Add(serverOffset);
        }

        public static DateTime? ToOriginalTimeZone(this DateTime? date, TimeSpan offsetFromUtc)
        {
            if (date == null)
                return null;

            return TimeZoneInfo.ConvertTimeToUtc(date.Value).Add(offsetFromUtc);
        }

        public static string DateToString(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
