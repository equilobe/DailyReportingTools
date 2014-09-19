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
        public Policy policy;
        public Options options;

        public Report(Policy p, Options o)
        {
            this.policy = p;
            this.options = o;
        }
        public string Title { get; set; }
        public List<Author> Authors { get; set; }
        public DateTime Date { get; set; }
        public Summary Summary { get; set; }
        public SprintTasks Sprint { get; set; }
    }
}
