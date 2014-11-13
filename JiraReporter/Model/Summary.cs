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
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportDate
        {
            get
            {
                if ((ToDate - FromDate).Days > 1)
                    return FromDate.ToString("m") + " - " + ToDate.ToString("m");
                else
                    return FromDate.DayOfWeek.ToString();
            }
        }

        public int TotalTimeSeconds { get; set; }
        public double TotalTimeHours {
            get
            {
                return TotalTimeSeconds / 3600;
            }
         }
        public string TotalTime
        {
            get
            {
                return TotalTimeHours.RoundDoubleDecimals();
            }
        }

        public int InProgressTasksCount { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public int InProgressUnassignedCount { get; set; }
        public int OpenTasksCount { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public int OpenUnassignedCount { get; set; }

        public int SprintTasksTimeLeftSeconds { get; set; }
        public double SprintTasksTimeLeftHours { get; set; }
        public string SprintTasksTimeLeftHoursString
        {
            get
            {
                return SprintTasksTimeLeftHours.RoundDoubleDecimals();
            }
        }
        public double SprintHourRate { get; set; }  //hour rate per day to complete sprint
        public string SprintHourRateString
        {
            get
            {
                return SprintHourRate.RoundDoubleDecimals();
            }
        }

        public int AuthorsInvolved { get; set; }
        public List<Author> Authors { get; set; }
        public int CommitsCount { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }

        public int AllocatedHoursPerDay { get; set; } //hour rate per day for developers team (set in policy)
        public int AllocatedHoursPerMonth { get; set; }
        public double MonthHoursWorked { get; set; }
        public string MonthHoursWorkedString
        {
            get
            {
                return MonthHoursWorked.RoundDoubleDecimals();
            }
        }
        public double RemainingMonthlyHours { get; set; }
        public double MonthHourRateHours { get; set; }
        public string MonthHourRateHoursString
        {
            get
            {
                return MonthHourRateHours.RoundDoubleDecimals();
            }
        }

        public Errors Errors { get; set; }

        public Summary(List<Author> authors, SprintTasks sprint, List<PullRequest> pullRequests, Policy policy, Dictionary<TimesheetType,Timesheet> timesheetCollection)
        {
            TotalTimeSeconds = TimeFormatting.GetReportTotalTime(authors);

            SetSummaryTasksTimeLeft(sprint);

            InProgressUnassignedCount = sprint.InProgressTasks.Count(tasks => tasks.Assignee == null);
            OpenUnassignedCount = sprint.OpenTasks.Count(tasks => tasks.Assignee == null);
            InProgressTasksCount = sprint.InProgressTasks.Count;
            OpenTasksCount = sprint.OpenTasks.Count;

            AuthorsInvolved = authors.Count;
            Authors = authors;
            CommitsCount = authors.Sum(a => a.Commits.Count);
            PullRequests = pullRequests;
            UnrelatedPullRequests = PullRequests.FindAll(p => p.TaskSynced == false);

            AllocatedHoursPerMonth = policy.AllocatedHoursPerMonth;
            AllocatedHoursPerDay = policy.AllocatedHoursPerDay;

            SetMonthSummary(timesheetCollection[TimesheetType.MonthTimesheet]);
            SprintTasksTimeLeftSeconds = GetSprintTimeLeftSeconds();
            SprintTasksTimeLeftHours = SprintTasksTimeLeftSeconds / 3600;
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
            if (AllocatedHoursPerMonth > 0)
                RemainingMonthlyHours = AllocatedHoursPerMonth - MonthHoursWorked;
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
