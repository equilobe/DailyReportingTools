using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class SprintTasks
    {
        public Dictionary<string, List<Issue>> CompletedTasks { get; set; }
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public List<Issue> UnassignedTasks { get; set; }
        public List<Issue> UncompletedTasks
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
