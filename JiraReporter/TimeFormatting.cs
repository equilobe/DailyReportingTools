using Equilobe.DailyReport.Models.Jira;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
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

        public static void SetAverageWorkStringFormat(Timing timing)
        {
            timing.AverageWorkedMonthString = (timing.AverageWorkedMonth / 3600).RoundDoubleOneDecimal();
            timing.AverageWorkedSprintString = (timing.AverageWorkedSprint / 3600).RoundDoubleOneDecimal();
            timing.AverageWorkedString = (timing.AverageWorked / 3600).RoundDoubleOneDecimal();
        }

        public static void SetaAverageRemainingStringFormat(TimingDetailed timing)
        {
            timing.RemainingSprintAverageString = (timing.RemainingSprintAverage / 3600).RoundDoubleOneDecimal();
            timing.RemainingMonthAverageString = (timing.RemainingMonthAverage / 3600).RoundDoubleOneDecimal();
        }

    }
}
