using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Author
    {
        public string Name { get; set; }
        public string Initials { get; set; }
      
        public string TimeLogged { get; set; }
        public int TimeSpent { get; set; }
        public int TimeSpentHours { get; set; }

        public int TimeSpentCurrentMonthSeconds { get; set; }
        public int TimeSpentCurrentMonthHours
        {
            get
            {
                return TimeSpentCurrentMonthSeconds / 3600;
            }
        }

        public List<Issue> Issues { get; set; }
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }

        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int RemainingEstimateSeconds { get; set; }
        public int RemainingEstimateHours
        {
            get
            {
                return RemainingEstimateSeconds / 3600;
            }
        }
        public List<Commit> Commits { get; set; }
        public List<DayLog> DayLogs { get; set; }
        public int ErrorsCount { get; set; }
    }
}
