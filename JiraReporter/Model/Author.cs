using SvnLogReporter.Model;
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
        public List<Issue> Issues { get; set; }
        public string TimeLogged { get; set; }
        public int TimeSpent { get; set; }
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public List<Commit> Commits { get; set; }
        public List<DayLog> DayLogs { get; set; }
    }
}
