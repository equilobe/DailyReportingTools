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
        public List<Task> InProgressTasks { get; set; }
        public List<Task> OpenTasks { get; set; }
        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public List<Commit> Commits { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<DayLog> DayLogs { get; set; }

        public int UnsyncedCommitsCount 
        { 
            get
            {
                if (Commits != null)
                    return Commits.Count(com => com.TaskSynced == false);
                else return 0;
            }
        }
        public int UnsyncedPullRequestsCount
        {
            get
            {
                if (PullRequests != null)
                    return PullRequests.Count(p => p.TaskSynced == false);
                else return 0;
            }
        }
    }
}
