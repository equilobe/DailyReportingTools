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
        public int InProgressUnassignedCount { get; set; }
        public int OpenTasksCount { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public int OpenUnassignedCount { get; set; }

        public int SprintTasksTimeLeftSeconds { get; set; }
        public int SprintTasksTimeLeftHours { get; set; }
        public string SprintTasksTimeLeft { get; set; }
        public int SprintHourRate { get; set; }  //hour rate per day to complete sprint

        public int MonthlyHourRate { get; set; } //hour rate per day for developers team (set in policy)

        public int AuthorsInvolved { get; set; }
        public int CommitsCount { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }

        public int MonthlyHours { get; set; }
        public int MonthHoursWorked { get; set; }
        public int RemainingMonthlyHours { get; set; }

        public int MonthHourRateHours { get; set; }

        public Errors Errors { get; set; }

        public Summary(List<Author> authors, SprintTasks sprint, List<PullRequest> pullRequests, Policy policy, Timesheet monthTimesheet)
        {
            TotalTimeSeconds = TimeFormatting.GetReportTotalTime(authors);
            TotalTime = TimeFormatting.SetTimeFormat(this.TotalTimeSeconds);

            SetSummaryTasksTimeLeft(sprint);

            InProgressUnassignedCount = sprint.InProgressTasks.Count(tasks => tasks.Assignee == null);
            OpenUnassignedCount = sprint.OpenTasks.Count(tasks => tasks.Assignee == null);
            InProgressTasksCount = sprint.InProgressTasks.Count;
            OpenTasksCount = sprint.OpenTasks.Count;

            AuthorsInvolved = authors.Count;
            CommitsCount = authors.Sum(a => a.Commits.Count);
            PullRequests = pullRequests;
            UnrelatedPullRequests = PullRequests.FindAll(p => p.TaskSynced == false);

            MonthlyHours = policy.AllocatedHoursPerMonth;
            if(MonthlyHours!=0)
              SetMonthSummary(monthTimesheet);
            SprintTasksTimeLeftSeconds = GetSprintTimeLeftSeconds();
            SprintTasksTimeLeftHours = SprintTasksTimeLeftSeconds / 3600;
            SprintTasksTimeLeft = TimeFormatting.SetTimeFormat(SprintTasksTimeLeftSeconds);
            SetHourRates();

            Errors = new Errors(authors, sprint);
        }   

        private void SetSummaryTasksTimeLeft(SprintTasks tasks)
        {
            InProgressTasksTimeLeftSeconds = 0;
            OpenTasksTimeLeftSeconds = 0;

            if (tasks.InProgressTasks != null)
                InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.InProgressTasks);
            if (tasks.OpenTasks != null)
                OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.OpenTasks);

            InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(InProgressTasksTimeLeftSeconds);
            OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(OpenTasksTimeLeftSeconds);
        }

        private void SetMonthSummary(Timesheet monthTimesheet)
        {
            MonthHoursWorked = monthTimesheet.Worklog.Issues.Sum(i => i.Entries.Sum(e => e.TimeSpent)) / 3600;
            RemainingMonthlyHours = MonthlyHours - MonthHoursWorked;
        }

        private void SetHourRates()
        {
            var days = GetWorkingDays(DateTime.Now);
            MonthHourRateHours = RemainingMonthlyHours / days;
            SprintHourRate = SprintTasksTimeLeftHours / days;
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

        private int GetSprintTimeLeftSeconds()
        {
            return OpenTasksTimeLeftSeconds + InProgressTasksTimeLeftSeconds;
        }
    }
}
