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
        public Policy Policy { get; set; }
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

        public int OpenUnassignedTasksSecondsLeft { get; set; }
        public string OpenUnassignedTasksTimeLeft
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(OpenUnassignedTasksSecondsLeft);
            }
        }

        public int InProgressUnassignedTasksSecondsLeft { get; set; }
        public string InProgressUnassignedTasksTimeLeft
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(InProgressUnassignedTasksSecondsLeft);
            }
        }

        public int UnassignedTasksSecondsLeft { get; set; }
        public string UnassignedTasksTimeLeft
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour(UnassignedTasksSecondsLeft);
            }
        }

        public int SprintTasksTimeLeftSeconds { get; set; }
        public double SprintTasksTimeLeftHours
        {
            get
            {
                return SprintTasksTimeLeftSeconds / 3600;
            }
        }
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
                return TimeFormatting.SetTimeFormat8Hour((int)(MonthHoursWorked * 3600));
            }
        }
        public double SprintHoursWorked { get; set; }
        public string SprintHoursWorkedString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(SprintHoursWorked * 3600)); ;
            }
        }
        public double SprintWorkedPerDay { get; set; }
        public string SprintWorkedPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(SprintWorkedPerDay));
            }
        }
        public int SprintTotalEstimate { get; set; }
        public string SprintTotalEstimateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat(SprintTotalEstimate);
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
        public double MonthWorkedPerDay { get; set; }
        public string MonthWorkedPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(MonthWorkedPerDay));
            }
        }
        public int MonthTotalEstimate { get; set; }
        public string MonthTotalEstimateString
        {
            get
            {
                return TimeFormatting.SetTimeFormat(MonthTotalEstimate);
            }
        }
        public string MonthTimeLeft
        {
            get
            {
                var days = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), NonWorkingDays);
                var seconds = days * 28800;
                return TimeFormatting.SetTimeFormat8Hour(seconds);
            }
        }
        public bool HasSprint
        {
            get
            {
                return SprintHoursWorked != 0 || SprintTasksTimeLeftSeconds != 0;
            }
        }

        public List<int> NonWorkingDays
        {
            get
            {
                if (Policy.OverrideThisMonth == true)
                    return Policy.CurrentOverride.NonWorkingDays;
                else
                    return new List<int>();
            }
        }

        public List<Error> Errors { get; set; }
        public List<Error> CompletedWithEstimateErrors { get; set; }
        public List<Error> CompletedWithNoWorkErrors { get; set; }
        public List<Error> UnassignedErrors { get; set; }
        public List<Author> AuthorsWithErrors { get; set; }

        public Health WorkedDaysHealth { get; set; }
        public Health DayHealth { get; set; }
        public Health SprintHealth { get; set; }
        public Health MonthHealth { get; set; }

        public Dictionary<Health, string> HealthColors { get; set; }
        public string SprintStatus { get; set; }
        public string MonthStatus { get; set; }

        public double SprintHourRateVariance { get; set; }
        public double MonthHourRateVariance { get; set; }

        public const int ChartMaxBarWidth = 250;
        public const int ChartMaxWidth = 300;

        public string ChartMaxWidthString
        {
            get
            {
                return ChartMaxWidth.ToString() + "px";
            }
        }


        public Summary(List<Author> authors, SprintTasks sprintTasks, List<PullRequest> pullRequests, Policy policy, Options options, Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            Policy = policy;
            AuthorsInvolved = authors.Count;
            Authors = authors;
            CommitsCount = authors.Sum(a => a.Commits.Count);
            PullRequests = pullRequests;
            UnrelatedPullRequests = PullRequests.FindAll(p => p.TaskSynced == false);
            SetDates(options);

            var reportWorkingDays = GetWorkingDays(FromDate, ToDate.AddDays(-1), NonWorkingDays);
            var monthWorkedDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1), NonWorkingDays);
            var monthWorkingDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), NonWorkingDays);
            var monthWorkingDaysLeft = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), NonWorkingDays);
            int sprintRemainingDays = 0;
            int sprintWorkedDays = 0;
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
            {
                var sprintEndDate = timesheetCollection[TimesheetType.SprintTimesheet].EndDate.ToOriginalTimeZone().AddDays(-1);
                var sprintStartDate = timesheetCollection[TimesheetType.SprintTimesheet].StartDate.ToOriginalTimeZone();
                sprintRemainingDays = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), sprintEndDate, NonWorkingDays);
                sprintWorkedDays = GetWorkingDays(sprintStartDate, sprintEndDate, NonWorkingDays);
            }
            SetTimeWorked(timesheetCollection);
            SetAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
            SetAllocatedTime();
            SetSummaryTasksTimeLeft(sprintTasks);

            GetTasksCount(sprintTasks);

            OpenUnassignedTasksSecondsLeft = JiraReporter.TasksService.GetTimeLeftForSpecificAuthorTasks(sprintTasks.OpenTasks, null);
            InProgressUnassignedTasksSecondsLeft = JiraReporter.TasksService.GetTimeLeftForSpecificAuthorTasks(sprintTasks.InProgressTasks, null);
            UnassignedTasksSecondsLeft = OpenUnassignedTasksSecondsLeft + InProgressUnassignedTasksSecondsLeft;

            SetRemainingMonthlyHours(timesheetCollection[TimesheetType.MonthTimesheet]);
            SprintTasksTimeLeftSeconds = GetSprintTimeLeftSeconds();
            SetHourRates(monthWorkingDaysLeft, sprintRemainingDays);

            SetHealthColors();
            SetVariances(sprintWorkedDays, monthWorkedDays, monthWorkingDays);

            SetHealth(timesheetCollection);
            SetHealthStatuses();

            SetAuthorsWorkSummaryChartWidths();

            SetErrors(sprintTasks);
        }

        private void GetTasksCount(SprintTasks sprintTasks)
        {
            InProgressUnassignedCount = sprintTasks.InProgressTasks.Count(tasks => tasks.Assignee == null);
            OpenUnassignedCount = sprintTasks.OpenTasks.Count(tasks => tasks.Assignee == null);
            InProgressTasksCount = sprintTasks.InProgressTasks.Count;
            OpenTasksCount = sprintTasks.OpenTasks.Count;
        }

        private void SetDates(Options options)
        {
            FromDate = options.FromDate;
            ToDate = options.ToDate;
        }

        private void SetAllocatedTime()
        {
            SetAllocatedTimePerDay();
            SetAllocatedTimePerMonth();
        }

        private void SetAllocatedTimePerDay()
        {
            if (Policy.OverrideThisMonth == true && Policy.CurrentOverride.AllocatedHoursPerDay > 0)
                AllocatedHoursPerDay = Policy.CurrentOverride.AllocatedHoursPerDay;
            else
                AllocatedHoursPerDay = Policy.AllocatedHoursPerDay;
        }

        private void SetAllocatedTimePerMonth()
        {
            if (Policy.OverrideThisMonth == true && Policy.CurrentOverride.AllocatedHoursPerMonth > 0)
                AllocatedHoursPerMonth = Policy.CurrentOverride.AllocatedHoursPerMonth;
            else
                AllocatedHoursPerMonth = Policy.AllocatedHoursPerMonth;
        }

        private void SetTimeWorked(Dictionary<TimesheetType, Timesheet> timesheetCollection)
        {
            TotalTimeSeconds = timesheetCollection[TimesheetType.ReportTimesheet].GetTimesheetSecondsWorked();
            MonthHoursWorked = (double)timesheetCollection[TimesheetType.MonthTimesheet].GetTimesheetSecondsWorked() / 3600;
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
                SprintHoursWorked = (double)timesheetCollection[TimesheetType.SprintTimesheet].GetTimesheetSecondsWorked() / 3600;
        }

        private void SetAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            SetTotalAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays);
            SetAuthorsAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
        }

        private void SetTotalAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays)
        {
            if (monthWorkedDays > 0)
                MonthWorkedPerDay = (MonthHoursWorked * 3600) / monthWorkedDays;
            if (sprintWorkedDays > 0)
                SprintWorkedPerDay = (SprintHoursWorked * 3600) / sprintWorkedDays;
        }

        private void SetAuthorsAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            foreach (var author in Authors)
                AuthorsProcessing.SetAuthorAverageWorkPerDay(author, monthWorkedDays, sprintWorkedDays, reportWorkingDays);
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

        private void SetHourRates(int monthWorkingDaysLeft, int sprintWorkingDaysLeft)
        {
            MonthHourRate = RemainingMonthHours / monthWorkingDaysLeft;
            if (sprintWorkingDaysLeft > 0)
                SprintHourRate = (double)SprintTasksTimeLeftHours / sprintWorkingDaysLeft;
        }

        public static int GetWorkingDays(DateTime startDate, DateTime endDate, List<int> nonWorkingDays)
        {
            DateTime dateIterator = startDate.Date;
            int days = 0;
            while (dateIterator <= endDate)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday && nonWorkingDays.Exists(d => d == dateIterator.Day) == false)
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
            var healthInspector = new HealthInspector(NonWorkingDays);
            WorkedDaysHealth = healthInspector.GetWorkedDaysHealth(AllocatedHoursPerDay * GetWorkingDays(FromDate, ToDate.AddDays(-1), NonWorkingDays), TotalTimeHours);
            if (timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
            {
                DayHealth = healthInspector.GetDayHealth(AllocatedHoursPerDay, SprintHourRate);
                SprintHealth = healthInspector.GetSprintHealth(timesheetCollection[TimesheetType.SprintTimesheet], AllocatedHoursPerDay, SprintHoursWorked);
            }
            else
            {
                DayHealth = Health.None;
                SprintHealth = Health.None;
            }
            MonthHealth = healthInspector.GetMonthHealth(AllocatedHoursPerMonth, MonthHoursWorked);
        }

        public void SetSprintVariance(int sprintWorkedDays)
        {
            SprintHourRateVariance = MathHelpers.GetVariance(AllocatedHoursPerDay * sprintWorkedDays, SprintHoursWorked);
        }

        public void SetMonthVariance(int monthWorkedDays, int monthWorkingDays)
        {
            var workedPerDay = MonthHoursWorked / monthWorkedDays;
            var averageFromAllocatedHours = AllocatedHoursPerMonth / monthWorkingDays;
            MonthHourRateVariance = MathHelpers.GetVariance(workedPerDay, averageFromAllocatedHours);
        }

        public void SetVariances(int sprintWorkedDays, int monthWorkedDays, int monthWorkingDays)
        {
            if (sprintWorkedDays > 0)
                SetSprintVariance(sprintWorkedDays);
            if (AllocatedHoursPerMonth > 0)
                SetMonthVariance(monthWorkedDays, monthWorkingDays);
        }

        private void SetHealthStatuses()
        {
            SprintStatus = HealthInspector.GetSprintStatus(SprintHealth, SprintHourRateVariance);
            MonthStatus = HealthInspector.GetMonthStatus(MonthHealth, MonthHourRateVariance);
        }

        private void SetErrors(SprintTasks tasks)
        {
            SetAuthorsWithErrors();
            GetCompletedTasksErrors(tasks);
            GetUnassignedErrors(tasks);
            GetAllErrors();
        }

        private void GetAllErrors()
        {
            var errors = new List<Error>();
            if (AuthorsWithErrors != null)
                errors = AuthorsWithErrors.SelectMany(e => e.Errors).ToList();
            if (CompletedWithEstimateErrors != null)
                errors = errors.Concat(CompletedWithEstimateErrors).ToList();
            if (CompletedWithNoWorkErrors != null)
                errors = errors.Concat(CompletedWithNoWorkErrors).ToList();
            if (UnassignedErrors != null)
                errors = errors.Concat(UnassignedErrors).ToList();
            Errors = errors;
        }

        private void SetAuthorsWithErrors()
        {
            AuthorsWithErrors = new List<Author>();
            AuthorsWithErrors = Authors.FindAll(a => a.Errors != null && a.Errors.Count > 0).ToList();
        }

        private void GetCompletedTasksErrors(SprintTasks tasks)
        {
            CompletedWithEstimateErrors = new List<Error>();
            CompletedWithNoWorkErrors = new List<Error>();
            if (tasks.CompletedTasks != null)
                foreach (var completedTasks in tasks.CompletedTasks.Values)
                {
                    var tasksWithErrors = completedTasks.Where(t => t.ErrorsCount > 0);
                    var errorsWithEstimate = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasRemaining)).ToList();
                    var errorsWithNoTimeSpent = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasNoTimeSpent)).ToList();
                    CompletedWithEstimateErrors = CompletedWithEstimateErrors.Concat(errorsWithEstimate).ToList();
                    CompletedWithNoWorkErrors = CompletedWithNoWorkErrors.Concat(errorsWithNoTimeSpent).ToList();
                }
        }

        private void GetUnassignedErrors(SprintTasks tasks)
        {
            UnassignedErrors = new List<Error>();
            if (tasks.UnassignedTasks != null && tasks.UnassignedTasks.Count > 0)
            {
                var errors = new List<Error>();
                errors = tasks.UnassignedTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
                UnassignedErrors = UnassignedErrors.Concat(errors).ToList();
            }
        }

        private int GetWorkSummaryMax()
        {
            var max = new List<int>();
            foreach(var author in Authors)
            {
                var maxFromAuthor = AuthorsProcessing.GetAuthorMaxAverage(author);
                max.Add(maxFromAuthor);
            }
            double maxHours = (double)max.Max() / 3600;
            return MathHelpers.RoundToNextEvenInteger(maxHours);
        }

        private void SetAuthorsWorkSummaryChartWidths()
        {
            var max = GetWorkSummaryMax();
            foreach (var author in Authors)
                AuthorsProcessing.SetAuthorWorkSummaryWidths(author, ChartMaxBarWidth, max);
        }
    }
}