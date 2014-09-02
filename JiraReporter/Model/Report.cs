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
        public string TotalTime { get; set; }
        public DateTime Date { get; set; }
        public List<Author> Summary { get; set; }
        public SprintReport SprintReport { get; set; }

        public void SetReportTimes()
        {
            Author.SetAuthorsTimeSpent(this.Authors);
            this.TotalTime = SetTotalTime(this.Authors);
            Author.SetAuthorsTimeFormat(this.Authors);
        }

        private string SetTotalTime(List<Author> authors)
        {
            int totalTime = 0;
            foreach (var author in authors)
                totalTime += author.TimeSpent;
            
            var totalTimeString = Timesheet.SetTimeFormat(totalTime);
            return totalTimeString;
        }
    }
}
