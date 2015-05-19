using Equilobe.DailyReport.Models.Policy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class WorkingDaysContext
    {
        public List<Month> MonthlyOptions { get; set; }
        public List<DayOfWeek> WeekendDaysList { get; set; }

        public WorkingDaysContext(List<Month> monthlyOptions, List<DayOfWeek> weekendDays)
        {
            MonthlyOptions = monthlyOptions;
            WeekendDaysList = weekendDays;
        }
    }
}
