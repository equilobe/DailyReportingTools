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
    }
}
