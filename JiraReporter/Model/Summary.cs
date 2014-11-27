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
        public double TotalTimeHours
        {
            get
            {
                return (double)TotalTimeSeconds / 3600;
            }
        }
        public string TotalTime
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(TotalTimeSeconds);
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
        public double SprintHourRate { get; set; }  //hour rate per day to complete sprint
        public string SprintHourRateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(SprintHourRate * 3600));
            }
        }

        public int AuthorsInvolved { get; set; }
        public List<Author> Authors { get; set; }
        public int CommitsCount { get; set; }
        public List<PullRequest> PullRequests { get; set; }
        public List<PullRequest> UnrelatedPullRequests { get; set; }

        public double AllocatedHoursPerDay { get; set; } //hour rate per day for developers team (set in policy)
        public string AllocatedHoursPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(AllocatedHoursPerDay * 3600));
            }
        }
        public double AllocatedHoursPerMonth { get; set; }
        public string AllocatedHoursPerMonthDetailed
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(AllocatedHoursPerMonth * 3600));
            }
        }
        public string AllocatedHoursPerMonthTime
        {
            get
            {
                return TimeFormatting.SetTimeFormat((int)(AllocatedHoursPerMonth * 3600));
            }
        }
        public double MonthHoursWorked { get; set; }
        public string MonthHoursWorkedString
        {
            get
            {
                return TimeFormatting.SetTimeFormat((int)(MonthHoursWorked * 3600));
            }
        }
        public double SprintHoursWorked { get; set; }
        public string SprintHoursWorkedString
        {
            get
            {
                return TimeFormatting.SetTimeFormat((int)(SprintHoursWorked * 3600)); ;
            }
        }
        public double RemainingMonthHours { get; set; }
        public string RemainingMonthHoursString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(RemainingMonthHours * 3600));
            }
        }
        public double MonthHourRate { get; set; }
        public string MonthHourRateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(MonthHourRate * 3600));
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

        public double SprintHourRateVariance { get; set; }
        public double MonthHourRateVariance { get; set; }

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
            SetVariances(timesheetCollection);

            SetHealth(timesheetCollection);
            SetHealthStatuses();

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
            MonthHourRate = RemainingMonthHours / monthDays;
            int sprintDays = 0;
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
                sprintDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), timesheetCollection[TimesheetType.SprintTimesheet].EndDate.ToOriginalTimeZone().AddDays(-1));          
            if (sprintDays > 0)
                SprintHourRate = (double)SprintTasksTimeLeftHours / sprintDays;
        }

        public static int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            DateTime dateIterator = startDate.Date;
            int days = 0;
            while (dateIterator <= endDate)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday)
                    days++;
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
            HealthColors.Add(Health.None, "#FFFFFF");
        }

        private void SetHealth(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            WorkedDaysHealth = HealthInspector.GetWorkedDaysHealth(AllocatedHoursPerDay * GetWorkingDays(FromDate, ToDate.AddDays(-1)), TotalTimeHours);
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
            {
                DayHealth = HealthInspector.GetDayHealth(AllocatedHoursPerDay, SprintHourRate);
                SprintHealth = HealthInspector.GetSprintHealth(timesheetCollection[TimesheetType.SprintTimesheet], AllocatedHoursPerDay, SprintHoursWorked);
            }
            else
                {
                    DayHealth = Health.None;
                    SprintHealth = Health.None;
                }
            MonthHealth = HealthInspector.GetMonthHealth(AllocatedHoursPerMonth, MonthHoursWorked);
        }

        public static double GetVariance(double allocatedTime, double hourRate)
        {
            return allocatedTime - hourRate;
        }

        public void SetSprintVariance(Timesheet sprint)
        {
            var workedDays = GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().AddDays(-1).Date);
            SprintHourRateVariance = GetVariance(AllocatedHoursPerDay * workedDays, SprintHoursWorked);
        }

        public void SetMonthVariance()
        {
            var workedDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1));
            var workedPerDay = MonthHoursWorked / workedDays;
            var monthWorkingDays = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().EndOfMonth());
            var averageFromAllocatedHours = AllocatedHoursPerMonth / monthWorkingDays;
            MonthHourRateVariance = GetVariance(averageFromAllocatedHours, workedPerDay);
        }

        public void SetVariances(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
                SetSprintVariance(timesheetCollection[TimesheetType.SprintTimesheet]);
            if(AllocatedHoursPerMonth > 0)
                SetMonthVariance();
        }

        private void SetHealthStatuses()
        {
            SprintStatus = HealthInspector.GetSprintStatus(SprintHealth, SprintHourRateVariance);
            MonthStatus = HealthInspector.GetMonthStatus(MonthHealth, MonthHourRateVariance);
        }
    }
}