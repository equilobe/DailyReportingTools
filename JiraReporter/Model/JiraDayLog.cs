using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class JiraDayLog
    {
        public DateTime Date { get; set; }
        public int TimeSpent { get; set; }
        public string TimeLogged { get; set; }
        public List<Issue> Issues { get; set; }
        public List<JiraCommit> Commits { get; set; }
        public string Title { get; set; }

        public List<JiraCommit> UnsyncedCommits { get; set; }
    }
}
