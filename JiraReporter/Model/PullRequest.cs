using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class PullRequest
    {
        public Octokit.PullRequest GithubPullRequest { get; set; }
        public bool TaskSynced { get; set; }
    }
}
