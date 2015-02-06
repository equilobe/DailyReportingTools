using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
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
        public string TotalTimeString   
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(TotalTimeSeconds);
            }
        }
        public string TimeLogged
        {
            get
            {
                return TotalTimeSeconds.SetTimeFormat();
            }
        }
        public int MonthSecondsWorked { get; set; }
        public double MonthHoursWorked { get; set; }
        public double SprintHoursWorked { get; set; }
        public int SprintSecondsWorked { get; set; }

        public double AverageWorked { get; set; }
        public string AverageWorkedString
        {
            get
            {
                var hoursWorked = AverageWorked / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }
        public double AverageWorkedSprint { get; set; }
        public string AverageWorkedSprintString
        {
            get
            {
                var hoursWorked = AverageWorkedSprint / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }
        public double AverageWorkedMonth { get; set; }
        public string AverageWorkedMonthString
        {
            get
            {
                return ((int)(AverageWorkedMonth)).SetTimeFormat8Hour();
            }
        }
        public string AverageWorkedMonthWithDecimals
        {
            get
            {
                var hoursWorked = AverageWorkedMonth / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }

        public int InProgressTasksTimeLeftSeconds { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
    }
}
