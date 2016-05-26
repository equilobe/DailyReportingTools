using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class Timing
    {
        public int TotalTimeSeconds { get; set; }
        public double TotalTimeHours
        {
            get
            {
                return (double)TotalTimeSeconds / 3600;
            }
        }
        public string TotalTimeString { get; set; }
        public string TimeLogged { get; set; }
        public int MonthSecondsWorked { get; set; }
        public double MonthHoursWorked { get; set; }
        public int Last7DaySecondsWorked { get; set; }
        public double Last7DaysHoursWorked
        {
            get
            {
                return (double)Last7DaySecondsWorked / 3600;
            }
        }
        public double SprintHoursWorked { get; set; }
        public int SprintSecondsWorked { get; set; }

        public double AverageWorked { get; set; }
        public double AverageWorkedHours
        {
            get
            {
                return AverageWorked / 3600;
            }
        }
        public string AverageWorkedString { get; set; }
        public double AverageWorkedSprint { get; set; }
        public double AverageWorkedSprintHours
        {
            get
            {
                return AverageWorkedSprint / 3600;
            }
        }
        public string AverageWorkedSprintString { get; set; }

        public double AverageWorkedLast7Days { get; set; }
        public double AverageWorkedLast7DaysHours
        {
            get
            {
                return AverageWorkedSprint / 3600;
            }
        }
        public string AverageWorkedLast7DaysString { get; set; }

        public double AverageWorkedMonth { get; set; }
        public double AverageWorkedMonthHours
        {
            get
            {
                return AverageWorkedMonth / 3600;
            }
        }
        public string AverageWorkedMonthString { get; set; }

        public int InProgressTasksTimeLeftSeconds { get; set; }
        public string InProgressTasksTimeLeftString { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public string OpenTasksTimeLeftString { get; set; }
        public int RemainingTasksTimeLeftSeconds { get; set; }
        public string RemainingTasksTimeLeftString { get; set; }

        public int TotalRemainingSeconds { get; set; }
        public double TotalRemainingHours { get; set; }
        public double TotalRemainingAverage { get; set; }
        public string TotalRemainingString { get; set; }
    }
}
