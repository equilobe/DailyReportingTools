using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class UncompletedTasks
    {
        public List<Issue> Issues { get; set; }
        public List<Issue> InProgressParents { get; set; }
        public List<Issue> OpenParents { get; set; }
        public List<Issue> UnassignedParents { get; set; }
    }
}
