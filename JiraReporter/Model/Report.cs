using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Report
    {
        public SvnLogReporter.Model.Policy policy;
        public SvnLogReporter.Options options;

        public Report(SvnLogReporter.Model.Policy p, SvnLogReporter.Options o)
        {
            this.policy = p;
            this.options = o;
        }
        public string Title { get; set; }
        public List<Author> Authors { get; set; }
        public DateTime Date { get; set; }
        public Summary Summary { get; set; }
        public SprintTasks Sprint { get; set; }
        public List<Octokit.PullRequest> PullRequests { get; set; } 
    }
}
