using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraPullRequest
    {
        public Octokit.PullRequest GithubPullRequest { get; set; }
        public bool TaskSynced { get; set; }
    }
}
