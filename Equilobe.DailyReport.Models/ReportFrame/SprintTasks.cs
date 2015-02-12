using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class SprintTasks
    {
        public Dictionary<string, List<CompleteIssue>> CompletedTasks { get; set; }
        public List<CompleteIssue> InProgressTasks { get; set; }
        public List<CompleteIssue> OpenTasks { get; set; }
        public List<CompleteIssue> UnassignedTasks { get; set; }
        public List<CompleteIssue> UncompletedTasks
        {
            get
            {
                return InProgressTasks.Concat(OpenTasks).ToList();
            }
        }
        public int UnassignedTasksErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }  
    }
}
