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
        public List<PullRequest> UnsyncedPullRequests { get; set; }

        public List<Commit> UnsyncedCommits { get; set; }

        public DayLog(Author author, List<Issue> issues, DateTime date)
        {
            this.Commits = AuthorsProcessing.GetDayLogCommits(author, date);
            this.UnsyncedCommits = new List<Commit>(Commits.FindAll(c => c.TaskSynced == false));
            this.Date = date;
            this.Title = TimeFormatting.GetStringDay(date);
            this.Title = this.Title.First().ToString().ToUpper() + this.Title.Substring(1);
          //  this.PullRequests = AuthorsProcessing.GetDayLogPullRequests(author);
            if(issues!=null)
                if(issues.Count>0)
                {
                    this.Issues = new List<Issue>();
                    foreach (var issue in issues)
                    {
                        this.Issues.Add(new Issue(issue));
                        IssueAdapter.RemoveWrongEntries(this.Issues.Last(), date);
                        IssueAdapter.SetIssueTimeSpent(this.Issues.Last());
                        IssueAdapter.SetIssueTimeFormat(this.Issues.Last());
                        this.TimeSpent += this.Issues.Last().Entries.Sum(x => x.TimeSpent);                      
                    }
                }            
            IssueAdapter.AdjustIssueCommits(this);
            IssueAdapter.RemoveWrongIssues(this.Issues);
            this.TimeLogged = TimeFormatting.SetTimeFormat(this.TimeSpent);          
        }        
    }
}
