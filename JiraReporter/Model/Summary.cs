using SourceControlLogReporter.Model;
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
        public int SprintTasksTimeLeftSeconds { get; set; }
        public string SprintTasksTimeLeft { get; set; }
        public int SprintHourRate { get; set; }  //hour rate per day to complete sprint

        public int MonthlyHourRate { get; set; } //hour rate per day for developers team (set in policy)

        public int AuthorsInvolved { get; set; }
        public int CommitsCount { get; set; }
        public int PullRequestsCount { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }
        public int MonthlyHours { get; set; }
        public string MonthlyHoursString { get; set; }
        public int MonthHoursWorked { get; set; }
        public string MonthHoursWorkedString { get; set; }
        public int RemainingMonthlyHours { get; set; }
        public string RemainingMonthlyHoursString { get; set; }
        public double AverageWorkRateToComplete { get; set; }
        public string AverageWorkRateToCompleteString { get; set; }

        public Summary(List<Author> authors, SprintTasks sprint, List<PullRequest> pullRequests, Policy policy, Timesheet monthTimesheet)
        {
            this.TotalTimeSeconds = TimeFormatting.GetReportTotalTime(authors);
            this.TotalTime = TimeFormatting.SetTimeFormat(this.TotalTimeSeconds);
            this.SetSummaryTasksTimeLeft(sprint);
            this.InProgressUnassigned = sprint.InProgressTasks.Count(tasks => tasks.Assignee == null);
            this.OpenUnassigned = sprint.OpenTasks.Count(tasks =>tasks.Assignee == null);
            this.InProgressTasksCount = sprint.InProgressTasks.Count;
            this.OpenTasksCount = sprint.OpenTasks.Count;
            this.AuthorsInvolved = authors.Count;
            this.CommitsCount = authors.Sum(a => a.Commits.Count);
            this.PullRequestsCount = pullRequests.Count;
            this.UnrelatedPullRequests = pullRequests.FindAll(p => p.TaskSynced == false);
            this.MonthlyHours = policy.AllocatedHoursPerMonth;
            this.MonthlyHoursString = TimeFormatting.SetTimeFormat(MonthlyHours * 3600);
            if(MonthlyHours!=0)
              SetMonthlyHours(monthTimesheet);
        }   

        private void SetSummaryTasksTimeLeft(SprintTasks tasks)
        {
            InProgressTasksTimeLeftSeconds = 0;
            OpenTasksTimeLeftSeconds = 0;
            //foreach (var author in authors)
            //{
            //    if(author.InProgressTasks!=null)
            //        this.InProgressTasksTimeLeftSeconds += IssueAdapter.GetTasksTimeLeftSeconds(author.InProgressTasks);
            //    if(author.OpenTasks!=null)
            //        this.OpenTasksTimeLeftSeconds += IssueAdapter.GetTasksTimeLeftSeconds(author.OpenTasks);
            //}
            if (tasks.InProgressTasks != null)
                InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.InProgressTasks);
            if (tasks.OpenTasks != null)
                OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.OpenTasks);

            InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(InProgressTasksTimeLeftSeconds);
            OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(OpenTasksTimeLeftSeconds);
        }

        private void SetMonthlyHours(Timesheet monthTimesheet)
        {
            MonthHoursWorked = monthTimesheet.Worklog.Issues.Sum(i => i.Entries.Sum(e => e.TimeSpent)) / 3600;
            MonthHoursWorkedString = TimeFormatting.SetTimeFormat(MonthHoursWorked * 3600);
            RemainingMonthlyHours = MonthlyHours - MonthHoursWorked;
            RemainingMonthlyHoursString = TimeFormatting.SetTimeFormat(RemainingMonthlyHours * 3600);
            SetAverageHourRate();
        }

        private void SetAverageHourRate()
        {
            var days = GetWorkingDays(DateTime.Now);
            AverageWorkRateToComplete = RemainingMonthlyHours / (double)days;
            AverageWorkRateToCompleteString = TimeFormatting.SetTimeFormat((int)(AverageWorkRateToComplete * 3600));
        }

        private int GetWorkingDays(DateTime date)
        {
            DateTime dateIterator = DateTime.Today;
            int days = 0;
            while(dateIterator < date.EndOfMonth())
            {
                if(dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday)
                {
                    days++;
                }
               dateIterator = dateIterator.AddDays(1);
            }
            return days;
        }
    }
}
