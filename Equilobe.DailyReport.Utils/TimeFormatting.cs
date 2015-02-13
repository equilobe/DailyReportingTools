using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Utils
{
    public static class TimeFormatting
    {
        public static string GetStringDay(DateTime date, DateTime reportDate)
        {
            var day = string.Empty;
            if(date.Date == reportDate.Date)
            {
                day = "Today";
            } 
            else if(date.Date == reportDate.Date.AddDays(-1))
            {
                day = "Yesterday";
            }
            else
            {
                day = date.DayOfWeek.ToString();
            } 
            return day;
        }

        public static TimeSpan SetTimeSpan(DateTime startDate, DateTime endDate)
        {
            TimeSpan timeAgo = endDate - startDate;
            return timeAgo;
        }

        public static string DateToString(DateTime date)
        {
            return date.ToString("dd/MMM/yyyy", DateTimeFormatInfo.InvariantInfo);
        }

        public static string DateToISO(DateTime date)
        {
            return date.ToString("yyyy'-'MM'-'dd' 'HH':'mm", DateTimeFormatInfo.InvariantInfo);
        }
    }
}
