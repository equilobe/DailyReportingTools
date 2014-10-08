using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class DayLog
    {
        public DateTime Date { get; set; }
        public int TimeSpent { get; set; }
        public string TimeLogged { get; set; }
        public List<Issue> Issues { get; set; }
        public List<Commit> Commits { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public string Title { get; set; }

        public DayLog(Author author, DateTime date)
        {
            if(author.Issues!=null)
                if(author.Issues.Count>0)
                {
                    this.Issues = new List<Issue>();
                    foreach (var issue in author.Issues)
                    {
                        this.Issues.Add(issue);
                        IssueAdapter.RemoveWrongEntries(this.Issues.Last(), date);
                        IssueAdapter.SetIssueTimeSpent(this.Issues.Last());
                        IssueAdapter.SetIssueTimeFormat(this.Issues.Last());
                        this.TimeSpent += this.Issues.Last().Entries.Sum(x => x.TimeSpent);                      
                    }
                }
            this.TimeLogged = TimeFormatting.SetTimeFormat8Hour(this.TimeSpent);
            this.Date = date;
            this.Title = TimeFormatting.GetStringDay(date);
        }        
    }
}
