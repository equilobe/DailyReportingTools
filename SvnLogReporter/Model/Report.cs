using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlLogReporter.Model
{
    public class Report
    {
        public string Title { get; set; }
        public IEnumerable<Author> Authors{get;set;}
        public DateTime ReportDate { get; set; }
        public List<PullRequest> PullRequests { get; set; }
    }
}
