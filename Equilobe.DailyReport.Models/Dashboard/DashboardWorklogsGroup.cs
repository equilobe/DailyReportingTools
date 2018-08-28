using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardWorklogsGroup
    {
        public DateTime Date { get; set; }
        public List<DashboardWorklog> WorklogGroup { get; set; }

        public long TotalTimeSpentInSeconds
        {
            get
            {
                var totalTime = 0L;

                if (!WorklogGroup.Any())
                    return totalTime;

                foreach (var worklog in WorklogGroup)
                    totalTime += worklog.TimeSpentInSeconds;

                return totalTime;
            }
        }

        // Xh Ym
        public string TotalTimeSpentHumanReadable
        {
            get
            {
                var hours = TotalTimeSpentInSeconds / 3600;
                var minutes = (TotalTimeSpentInSeconds % 3600) / 60;

                return (minutes == 0) ? hours + "h" : hours + "h " + minutes + "m";
            }
        }

        public string DayHumanReadable
        {
            get
            {
                return Date.ToString("dd/MMM");
            }
        }
    }
}
