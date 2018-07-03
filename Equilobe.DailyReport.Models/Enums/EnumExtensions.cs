using System;

namespace Equilobe.DailyReport.Models.Enums
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string enumType, bool ignoreCase = false)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), enumType, ignoreCase);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
