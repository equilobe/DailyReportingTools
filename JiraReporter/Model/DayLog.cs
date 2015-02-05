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
        public string Title { get; set; }

        public List<Commit> UnsyncedCommits { get; set; }

        public DayLog(Author author, DateTime date, JiraOptions options)
        {
            this.Commits = AuthorHelpers.GetDayLogCommits(author, date);
            this.Date = date;
            this.Title = TimeFormatting.GetStringDay(date);

            if (author.Issues != null)
                if (author.Issues.Count > 0)
                {
                    this.Issues = new List<Issue>();
                    foreach (var issue in author.Issues)
                    {
                        this.Issues.Add(new Issue(issue));
                        IssueAdapter.RemoveWrongEntries(this.Issues.Last(), date);
                        IssueAdapter.TimeSpentFromEntries(this.Issues.Last());
                        IssueAdapter.SetTimeFormat(this.Issues.Last());
                        this.TimeSpent += this.Issues.Last().TimeSpent;
                    }
                }
            IssueAdapter.AdjustIssueCommits(this);
            IssueAdapter.RemoveWrongIssues(this.Issues);
            
            if(Issues != null)
                  Issues = TasksService.GetParentTasks(Issues,author);
            this.UnsyncedCommits = new List<Commit>(Commits.FindAll(c => c.TaskSynced == false));
            this.TimeLogged = TimeFormatting.SetTimeFormat(this.TimeSpent);
        }      
    }
}
