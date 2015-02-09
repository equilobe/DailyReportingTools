using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraReport
    {
        public JiraPolicy Policy;
      //  public JiraOptions Options;

        public JiraReport(JiraPolicy p)
        {
            Policy = p;
        }
        public string Title { get; set; }
        public List<JiraAuthor> Authors { get; set; }
        public DateTime Date { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Summary Summary { get; set; }
        public SprintTasks SprintTasks { get; set; }
        public List<JiraPullRequest> PullRequests { get; set; }
        public List<JiraPullRequest> UnrelatedPullRequests
        {
            get
            {
                return PullRequests.FindAll(p => p.TaskSynced == false);
            }
        }
    }
}
