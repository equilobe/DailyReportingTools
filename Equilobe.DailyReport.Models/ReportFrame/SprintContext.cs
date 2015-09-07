using Equilobe.DailyReport.Models.Jira;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class SprintContext
    {
        public Sprint ReportSprint { get; set; }
        public Sprint FutureSprint { get; set; }
        public Sprint PastSprint { get; set; }
    }
}
