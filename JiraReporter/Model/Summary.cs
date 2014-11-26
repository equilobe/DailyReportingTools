using SourceControlLogReporter;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public enum Health { Bad, Weak, Good, None };
    public class Summary
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportDate
        {
            get
            {
                if ((ToDate - FromDate).Days > 1)
                    return FromDate.ToString("m") + " - " + ToDate.AddDays(-1).ToString("m");
                else
                    return FromDate.DayOfWeek.ToString();
            }
        }

        public int TotalTimeSeconds { get; set; }
        public int TotalTimeHours
        {
            get
            {
                return TotalTimeSeconds / 3600;
            }
        }
        public string TotalTime
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(TotalTimeHours * 3600);
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
        public int SprintTasksTimeLeftHours { get; set; }
        public string SprintTasksTimeLeftHoursString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(SprintTasksTimeLeftSeconds);
            }
        }
        public int SprintHourRate { get; set; }  //hour rate per day to complete sprint
        public string SprintHourRateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(SprintHourRate * 3600);
            }
        }

        public int AuthorsInvolved { get; set; }
        public List<Author> Authors { get; set; }
        public int CommitsCount { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }

        public int AllocatedHoursPerDay { get; set; } //hour rate per day for developers team (set in policy)
        public string AllocatedHoursPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(AllocatedHoursPerDay * 3600);
            }
        }
        public int AllocatedHoursPerMonth { get; set; }
        public string AllocatedHoursPerMonthDetailed
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(AllocatedHoursPerMonth * 3600);
            }
        }
        public string AllocatedHoursPerMonthTime
        {
            get
            {
                return TimeFormatting.SetTimeFormat(AllocatedHoursPerMonth * 3600);
            }
        }
        public int MonthHoursWorked { get; set; }
        public string MonthHoursWorkedString
        {
            get
            {
                return TimeFormatting.SetTimeFormat(MonthHoursWorked * 3600);
            }
        }
        public int SprintHoursWorked { get; set; }
        public string SprintHoursWorkedString
        {
            get
            {
                return TimeFormatting.SetTimeFormat(SprintHoursWorked * 3600); ;
            }
        }
        public int RemainingMonthHours { get; set; }
        public string RemainingMonthHoursString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(RemainingMonthHours * 3600);
            }
        }
        public int MonthHourRate { get; set; }
        public string MonthHourRateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(MonthHourRate * 3600);
            }
        }
        public bool HasSprint
        {
            get
            {
                return SprintHoursWorked != 0 || SprintTasksTimeLeftSeconds != 0;
            }
        }

        public Errors Errors { get; set; }

        public Health WorkedDaysHealth { get; set; }
        public Health DayHealth { get; set; }
        public Health SprintHealth { get; set; }
        public Health MonthHealth { get; set; }

        public Dictionary<Health, string> HealthColors { get; set; }
        public string SprintStatus { get; set; }
        public string MonthStatus { get; set; }

        public int SprintHourRateDeviation { get; set; }
        public int MonthHourRateDeviation { get; set; }

        public Summary(List<Author> authors, SprintTasks sprintTasks, List<PullRequest> pullRequests, Policy policy, Options options, Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            SetDates(options);
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
            SetHourRates(timesheetCollection);

            SetHealthColors();

            SetHealth(timesheetCollection);

            Errors = new Errors(authors, sprintTasks);
        }

        private void SetDates(Options options)
        {
            FromDate = options.FromDate;
            ToDate = options.ToDate;
        }

        private void SetTimeWorked(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            TotalTimeSeconds = timesheetCollection[TimesheetType.ReportTimesheet].GetTimesheetSecondsWorked();
            MonthHoursWorked = timesheetCollection[TimesheetType.MonthTimesheet].GetTimesheetSecondsWorked() / 3600;
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
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
                RemainingMonthHours = AllocatedHoursPerMonth - MonthHoursWorked;
        }

        private void SetHourRates(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            var monthDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().EndOfMonth());
            int sprintDays = 0;
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
                sprintDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), timesheetCollection[TimesheetType.SprintTimesheet].EndDate.ToOriginalTimeZone().AddDays(-1));
            MonthHourRate = RemainingMonthHours / monthDays;
            if (sprintDays > 0)
                SprintHourRate = SprintTasksTimeLeftHours / sprintDays;
        }

        public int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            DateTime dateIterator = startDate.Date;
            int days = 0;
            while (dateIterator < endDate)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday)
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

        private void SetHealthColors()
        {
            HealthColors = new Dictionary<Health, string>();
            HealthColors.Add(Health.Bad, "#FFE7E7");
            HealthColors.Add(Health.Weak, "#FFD");
            HealthColors.Add(Health.Good, "#DDFADE");
            HealthColors.Add(Health.None, "White");
        }

        private void SetHealth(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            WorkedDaysHealth = HealthInspector.GetWorkedDaysHealth(AllocatedHoursPerDay * GetWorkingDays(FromDate, ToDate), TotalTimeHours);
            DayHealth = HealthInspector.GetDayHealth(timesheetCollection, AllocatedHoursPerDay, SprintHourRate);
        }

        public static int GetDeviation(int allocatedTime, int hourRate)
        {
            return allocatedTime - hourRate;
        }
    }
}