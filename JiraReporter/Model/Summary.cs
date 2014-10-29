using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Summary
    {
        public string TotalTime { get; set; }
        public int TotalTimeSeconds { get; set; }
        public int InProgressTasksCount { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public int InProgressUnassigned { get; set; }
        public int OpenTasksCount { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public int OpenUnassigned { get; set; }
        public int AuthorsInvolved { get; set; }
        public int CommitsCount { get; set; }
        public int PullRequestsCount { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }
        public int MonthlyHours { get; set; }
        public int RemainingMonthlyHours { get; set; }
        public int AverageWorkRateToComplete { get; set; }

        public Summary(List<Author> authors, SprintTasks sprint, List<PullRequest> pullRequests, SvnLogReporter.Model.Policy policy)
        {
            this.TotalTimeSeconds = TimeFormatting.GetReportTotalTime(authors);
            this.TotalTime = TimeFormatting.SetTimeFormat(this.TotalTimeSeconds);
            this.SetSummaryTasksTimeLeft(authors);
            this.InProgressUnassigned = sprint.InProgressTasks.Count(tasks => tasks.Assignee == null);
            this.OpenUnassigned = sprint.OpenTasks.Count(tasks =>tasks.Assignee == null);
            this.InProgressTasksCount = sprint.InProgressTasks.Count;
            this.OpenTasksCount = sprint.OpenTasks.Count;
            this.AuthorsInvolved = authors.Count;
            this.CommitsCount = authors.Sum(a => a.Commits.Count);
            this.PullRequestsCount = pullRequests.Count;
            this.UnrelatedPullRequests = pullRequests.FindAll(p => p.TaskSynced == false);
            this.MonthlyHours = policy.AllocatedHoursPerMonth;
        }

        private void SetSummaryTasksTimeLeft(List<Author> authors)
        {
            this.InProgressTasksTimeLeftSeconds = 0;
            this.OpenTasksTimeLeftSeconds = 0;
            foreach (var author in authors)
            {
                if(author.InProgressTasks!=null)
                    this.InProgressTasksTimeLeftSeconds += IssueAdapter.GetTasksTimeLeftSeconds(author.InProgressTasks);
                if(author.OpenTasks!=null)
                    this.OpenTasksTimeLeftSeconds += IssueAdapter.GetTasksTimeLeftSeconds(author.OpenTasks);
            }

            this.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(this.InProgressTasksTimeLeftSeconds);
            this.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(this.OpenTasksTimeLeftSeconds);
        }
    }
}
