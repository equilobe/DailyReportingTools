using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public enum Health { Bad, Decent, Good};
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

        public double TotalTimeSeconds { get; set; }
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

        public double SprintTasksTimeLeftSeconds { get; set; }
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
        public double SprintHoursWorked { get; set; }
        public string SprintHoursWorkedString
        {
            get
            {
                return SprintHoursWorked.RoundDoubleDecimals();
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

        public Health WorkedDaysHealth { get; set; }
        public Health DayHealth { get; set; }
        public Health SprintHealth { get; set; }
        public Health MonthHealth { get; set; }

        public Summary(List<Author> authors, SprintTasks sprintTasks, List<PullRequest> pullRequests, Policy policy, Dictionary<TimesheetType,Timesheet> timesheetCollection)
        {
            SetTimeWorked(timesheetCollection);

            SetSummaryTasksTimeLeft(sprintTasks);

            InProgressUnassignedCount = sprintTasks.InProgressTasks.Count(tasks => tasks.Assignee == null);
            OpenUnassignedCount = sprintTasks.OpenTasks.Count(tasks => tasks.Assignee == null);
            InProgressTasksCount = sprintTasks.InProgressTasks.Count;
            OpenTasksCount = sprintTasks.OpenTasks.Count;

            AuthorsInvolved = authors.Count;
            Authors = authors;
            CommitsCount = authors.Sum(a => a.Commits.Count);
            PullRequests = pullRequests;
            UnrelatedPullRequests = PullRequests.FindAll(p => p.TaskSynced == false);

            AllocatedHoursPerMonth = policy.AllocatedHoursPerMonth;
            AllocatedHoursPerDay = policy.AllocatedHoursPerDay;

            SetRemainingMonthlyHours(timesheetCollection[TimesheetType.MonthTimesheet]);
            SprintTasksTimeLeftSeconds = GetSprintTimeLeftSeconds();
            SprintTasksTimeLeftHours = SprintTasksTimeLeftSeconds / 3600;
            SetHourRates();

            Errors = new Errors(authors, sprintTasks);
        }

        private void SetTimeWorked(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            TotalTimeSeconds = timesheetCollection[TimesheetType.ReportTimesheet].GetTimesheetSecondsWorked();
            MonthHoursWorked = timesheetCollection[TimesheetType.MonthTimesheet].GetTimesheetSecondsWorked() / 3600;
            SprintHoursWorked = timesheetCollection[TimesheetType.SprintTimesheet].GetTimesheetSecondsWorked() / 3600;
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

        private void SetRemainingMonthlyHours(Timesheet monthTimesheet)
        {
            if (AllocatedHoursPerMonth > 0)
                RemainingMonthlyHours = AllocatedHoursPerMonth - MonthHoursWorked;
        }

        private void SetHourRates()
        {
            var days = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().EndOfMonth());
            MonthHourRateHours = RemainingMonthlyHours / days;
            SprintHourRate = SprintTasksTimeLeftHours / days;
        }

        private int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            DateTime dateIterator = DateTime.Now.ToOriginalTimeZone().Date;
            int days = 0;
            while(dateIterator < endDate)
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

        //private void SetHealth()
        //{

        //}

        //private void SetWorkedDaysHealth()
        //{

        //}
    }
}
