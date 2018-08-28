using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardWorklogs
    {
        public DateTime Date { get; set; }
        public List<Worklog> WorklogGroup { get; set; }

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


    }
}
