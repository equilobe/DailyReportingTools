using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Errors
    {
        public int ErrorsCount { get; set; }
        public List<Author> AuthorsContainingErrors { get; set; }
        public int UnassignedErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }

        public Errors(List<Author> authors, SprintTasks sprint)
        {
            ErrorsCount += authors.Sum(a => a.ErrorsCount) + sprint.UnassignedTasksErrorCount + sprint.CompletedTasksErrorCount;
            AuthorsContainingErrors = authors.Where(a => a.ErrorsCount > 0).ToList();
            UnassignedErrorCount = sprint.UnassignedTasksErrorCount;
            CompletedTasksErrorCount = sprint.CompletedTasksErrorCount;
        }
    }
}
