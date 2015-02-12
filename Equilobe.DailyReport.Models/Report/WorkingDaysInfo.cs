using Equilobe.DailyReport.Models.Report;
using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Report
{
    public class WorkingDaysInfo
    {
        public int ReportWorkingDays { get; set; }
        public int MonthWorkingDays { get; set; }
        public int MonthWorkingDaysLeft { get; set; }
        public int MonthWorkedDays { get; set; }
        public int SprintWorkingDays { get; set; }
        public int SprintWorkingDaysLeft { get; set; }
        public int SprintWorkedDays { get; set; }
    }
}
