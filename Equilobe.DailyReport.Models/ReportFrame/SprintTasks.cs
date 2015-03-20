using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class SprintTasks
    {
        public Dictionary<string, List<IssueDetailed>> CompletedTasks { get; set; }
        public List<IssueDetailed> InProgressTasks { get; set; }
        public List<IssueDetailed> OpenTasks { get; set; }
        public List<IssueDetailed> UnassignedTasks { get; set; }
        public List<IssueDetailed> UncompletedTasks { get; set; }
        public int UnassignedTasksErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }  
    }
}
