using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraDayLog
    {
        public DateTime Date { get; set; }
        public int TimeSpent { get; set; }
        public string TimeLogged { get; set; }
        public List<IssueDetailed> Issues { get; set; }
        public List<JiraCommit> Commits { get; set; }
        public string Title { get; set; }

        public List<JiraCommit> UnsyncedCommits { get; set; }
    }
}
