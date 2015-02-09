using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class TimeFormattingExtensions
    {
        public static string SetTimeFormat(this int time)
        {
            string timeFormat = "";
            if (time < 0)
            {
                time = time * (-1);
                timeFormat += "- ";
            }
            TimeSpan t = TimeSpan.FromSeconds(time);

            if (t.Hours > 0 || t.Days > 0)
                timeFormat += string.Format("{0}h ", t.Hours + t.Days * 24);
            if (t.Minutes > 0)
                timeFormat += string.Format("{0}m", t.Minutes);
            if (t.Days == 0 && t.Hours == 0 && t.Minutes == 0)
                timeFormat = string.Format("{0}m", t.Minutes);
            if (timeFormat.Last() == ' ')
                timeFormat = timeFormat.Remove(timeFormat.Length - 1);
            return timeFormat;
        }

        public static string SetTimeFormat8Hour(this int seconds)
        {
            string timeFormat = "";
            if (seconds < 0)
            {
                seconds = seconds * (-1);
                timeFormat += "- ";
            }
            int weeks = seconds / 144000;
            int days = seconds / 28800 - weeks * 5;
            int hours = seconds / 3600 - weeks * 5 * 8 - days * 8;
            int minutes = seconds / 60 - weeks * 5 * 8 * 60 - days * 8 * 60 - hours * 60;

            if (weeks > 0)
                timeFormat += string.Format("{0}w ", weeks);
            if (days > 0)
                timeFormat += string.Format("{0}d ", days);
            if (hours > 0)
                timeFormat += string.Format("{0}h ", hours);
            if (minutes > 0)
                timeFormat += string.Format("{0}m", minutes);
            if (weeks == 0 && days == 0 && hours == 0 && minutes == 0)
                timeFormat = "0m";
            if (timeFormat.Last() == ' ')
                timeFormat = timeFormat.Remove(timeFormat.Length - 1);
            return timeFormat;
        }
    }
}
