using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class TimingDetailed : Timing
    {
        public int OpenUnassignedTasksSecondsLeft { get; set; }
        public string OpenUnassignedTasksTimeLeftString { get; set; }

        public int InProgressUnassignedTasksSecondsLeft { get; set; }
        public string InProgressUnassignedTasksTimeLeftString { get; set; }

        public int UnassignedTasksSecondsLeft { get; set; }
        public string UnassignedTasksTimeLeftString { get; set; }

        public double AllocatedHoursPerDay { get; set; }
        public double AllocatedHoursPerMonth { get; set; }

        public double SprintAverageEstimate { get; set; }
        public string SprintAverageEstimateString { get; set; }

        public double MonthAverageEstimated { get; set; }
        public string MonthAverageEstimatedString { get; set; }

        public double RemainingMonthHours { get; set; }
        public double RemainingMonthAverage { get; set; }
        public string RemainingMonthAverageString { get; set; }

        public double RemainingSprintAverage { get; set; }
        public string RemainingSprintAverageString { get; set; }
    }
}
