using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class TimingDetailed : Timing
    {
        public int OpenUnassignedTasksSecondsLeft { get; set; }
        public string OpenUnassignedTasksTimeLeftString 
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(OpenUnassignedTasksSecondsLeft);
            }
        }

        public int InProgressUnassignedTasksSecondsLeft { get; set; }
        public string InProgressUnassignedTasksTimeLeftString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(InProgressUnassignedTasksSecondsLeft);
            }
        }

        public int UnassignedTasksSecondsLeft { get; set; }
        public string UnassignedTasksTimeLeftString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(UnassignedTasksSecondsLeft);
            }
        }

        public double SprintTasksTimeLeftPerDay { get; set; }
        public string SprintTasksTimeLeftPerDayWithDecimals
        {
            get
            {
                var hoursLeft = SprintTasksTimeLeftPerDay / 3600;
                return hoursLeft.RoundDoubleOneDecimal();
            }
        }

        public double HourRateToCompleteSprint { get; set; } 
        public string HourRateToCompleteSprintString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(HourRateToCompleteSprint * 3600));
            }
        }
        public double HourRateToCompleteMonth { get; set; }
        public string HourRateToCompleteMonthString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(HourRateToCompleteMonth * 3600));
            }
        }

        public double AllocatedHoursPerDay { get; set; }
        public double AllocatedHoursPerMonth { get; set; }

        public double SprintAverageEstimate
        {
            get
            {
                return AllocatedHoursPerDay * 3600;
            }
        }
        public string SprintAverageEstimateWithDecimals
        {
            get
            {
                var estimatedHours = SprintAverageEstimate / 3600;
                return estimatedHours.RoundDoubleOneDecimal();
            }
        }

        public double MonthAverageEstimated { get; set; }
        public string MonthAverageEstimatedWithDecimals
        {
            get
            {
                var hoursEstimated = MonthAverageEstimated / 3600;
                return hoursEstimated.RoundDoubleOneDecimal();
            }
        }

        public double RemainingMonthHours { get; set; }
        public double RemainingMonthAverage { get; set; }
        public string RemainingMonthHoursAverageString
        {
            get
            {
                var remainingHours = RemainingMonthAverage / 3600;
                return remainingHours.RoundDoubleOneDecimal();
            }
        }
    }
}
