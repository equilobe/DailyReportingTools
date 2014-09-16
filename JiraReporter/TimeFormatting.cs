using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class TimeFormatting
    {
        public static string SetTimeFormat(int time)
        {
            string timeFormat = "";

            TimeSpan t = TimeSpan.FromSeconds(time);

            if (t.Hours > 0 || t.Days > 0)
                timeFormat += string.Format("{0}h", t.Hours + t.Days * 24) + " ";
            if (t.Minutes > 0)
                timeFormat += string.Format("{0}m", t.Minutes);
            if (t.Days == 0 && t.Hours == 0 && t.Minutes == 0)
                timeFormat = string.Format("{0}m", t.Minutes);
            return timeFormat;
        }

        public static string SetTimeFormat8Hour(int seconds)
        {
            string timeFormat = "";
            int days = seconds / 28800;
            int hours = seconds / 3600 - days * 8;
            int minutes = seconds / 60 - days * 8 * 60 - hours * 60;

            if (days > 0)
                timeFormat += string.Format("{0}d ", days);
            if (hours > 0)
                timeFormat += string.Format("{0}h", hours);
            if (minutes > 0)
                timeFormat += string.Format("{0}m", minutes);
            if (days == 0 && hours == 0 && minutes == 0)
                timeFormat = "0m";
            return timeFormat;
        }

        public static string GetCompletedTime(DateTime completedDate)
        {
            var completedTime = "";
            completedDate = new DateTime(completedDate.Year, completedDate.Month, completedDate.Day, 0, 0, 0);            
            int days = (int)SetTimeSpan(completedDate).TotalDays;
            var d = TimeSpan.FromDays(days);
            if (d.Days == 0)
                completedTime = "today";
            else
                if (d.Days == 1)
                    completedTime = "yesterday";
                else
                    completedTime = string.Format("{0} days ago", d.Days);
            return completedTime;
        }

        private static TimeSpan SetTimeSpan(DateTime date)
        {
            var dateNow = DateTime.Today;
            var resolutionDate = date;
            TimeSpan timeAgo = dateNow - date;
            return timeAgo;
        }


        public static string SetReportTotalTime(List<Author> authors)
        {
            int totalTime = 0;
            foreach (var author in authors)
                totalTime += author.TimeSpent;

            var totalTimeString = SetTimeFormat(totalTime);
            return totalTimeString;
        }

    }
}
