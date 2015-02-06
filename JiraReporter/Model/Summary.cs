using Equilobe.DailyReport.Models.ReportPolicy;
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
        public JiraPolicy Policy { get; set; }
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
        public int UnassignedTasksCount { get; set; }

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
        public double SprintTasksTimeLeftPerDay { get; set; }
        public string SprintTasksTimeLeftPerDayWithDecimals
        {
            get
            {
                var hoursLeft = SprintTasksTimeLeftPerDay / 3600;
                return hoursLeft.RoundDoubleOneDecimal();
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
        public List<JiraPullRequest> PullRequests { get; set; }
        public List<JiraPullRequest> UnrelatedPullRequests { get; set; }
        public Sprint Sprint { get; set; }

        public double AverageTimeWorked { get; set; }
        public string AverageTimeWorkedWithDecimals
        {
            get
            {
                var hoursWorked = AverageTimeWorked / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }

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
        public string SprintWorkedPerDayWithDecimals
        {
            get
            {
                var hoursWorked = SprintWorkedPerDay / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }

        public double SprintAverageEstimate
        {
            get
            {
                return Policy.AllocatedHoursPerDay * 3600;
            }
        }
        public string SprintAverageEstimateWithDecimals
        {
            get
            {
                var estimatedHours = SprintAverageEstimate / 3600;
                return estimatedHours.RoundDoubleOneDecimal();
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
        public double RemainingMonthAverage { get; set; }
        public string RemainingMonthHoursAverageWithDecimals
        {
            get
            {
                var remainingHours = RemainingMonthAverage / 3600;
                return remainingHours.RoundDoubleOneDecimal();
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
        public string MonthWorkedPerDayWithDecimals
        {
            get
            {
                var hoursWorked = MonthWorkedPerDay / 3600;
                return hoursWorked.RoundDoubleOneDecimal();
            }
        }
        public double MonthAverageEstimated { get; set; }
        public string MonthAverageEstimatedWithDecimals
        {
            get
            {
                var hoursEstimated = MonthAverageEstimated / 3600;
                return hoursEstimated.RoundDoubleOneDecimal();
            }
        }
        public int MonthEstimatedPixelWidth { get; set; }
        public string MonthEstimatedPixelWidthString
        {
            get
            {
                return MonthEstimatedPixelWidth.ToString() + "px";
            }
        }
        public string MonthTimeLeft
        {
            get
            {
                var days = GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), Policy.MonthlyOptions);
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

        public List<Error> Errors { get; set; }
        public List<Error> CompletedWithEstimateErrors { get; set; }
        public List<Error> CompletedWithNoWorkErrors { get; set; }
        public List<Error> UnassignedErrors { get; set; }
        public List<Error> ConfirmationErrors { get; set; }
        public List<Author> AuthorsNotConfirmed { get; set; }
        public List<Author> AuthorsWithErrors { get; set; }

        public WorkingDaysInfo WorkingDays { get; set; }

        public Health WorkedDaysHealth { get; set; }
        public Health DayHealth { get; set; }
        public Health SprintHealth { get; set; }
        public Health MonthHealth { get; set; }

        public Dictionary<Health, string> HealthColors { get; set; }
        public string SprintStatus { get; set; }
        public string MonthStatus { get; set; }

        public StatusChartWidths SprintWidths { get; set; }
        public StatusChartWidths MonthWidths { get; set; }

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


        public Summary(List<Author> authors, SprintTasks sprintTasks, List<JiraPullRequest> pullRequests, JiraPolicy policy, JiraOptions options, Sprint sprint)
        {
            Policy = policy;
            AuthorsInvolved = authors.Count;
            Authors = authors;
            CommitsCount = authors.Where(a => a.Commits != null).Sum(a => a.Commits.Count);
            PullRequests = pullRequests;
            Sprint = sprint;
            UnrelatedPullRequests = PullRequests.FindAll(p => p.TaskSynced == false);
            UnassignedTasksCount = sprintTasks.UnassignedTasks.Count(t => t.IsSubtask == false);
            SetDates(options);
            SetAllocatedTime();
            WorkingDays = new WorkingDaysInfo(Sprint, policy, options);

            SetTimeWorked();
            SetMonthEstimatedValue(WorkingDays.MonthWorkingDays);
            SetAverageTimeWorkedPerDay(WorkingDays.MonthWorkedDays, WorkingDays.SprintWorkedDays, WorkingDays.ReportWorkingDays);
            SetSummaryTasksTimeLeft(sprintTasks);
            SprintTasksTimeLeftSeconds = GetSprintTimeLeftSeconds();
            SetRemainingMonthlyHours();
            SetAverageRemainingTimePerDay(WorkingDays.MonthWorkingDays, WorkingDays.SprintWorkingDaysLeft);

            GetTasksCount(sprintTasks);

            OpenUnassignedTasksSecondsLeft = JiraReporter.TasksService.GetTimeLeftForSpecificAuthorTasks(sprintTasks.OpenTasks, null);
            InProgressUnassignedTasksSecondsLeft = JiraReporter.TasksService.GetTimeLeftForSpecificAuthorTasks(sprintTasks.InProgressTasks, null);
            UnassignedTasksSecondsLeft = OpenUnassignedTasksSecondsLeft + InProgressUnassignedTasksSecondsLeft;

            SetHourRates(WorkingDays.MonthWorkingDaysLeft, WorkingDays.SprintWorkingDaysLeft);

            SetHealthColors();
            SetVariances(WorkingDays.SprintWorkedDays, WorkingDays.MonthWorkedDays, WorkingDays.MonthWorkingDays);

            SetHealth(WorkingDays.SprintWorkedDays);
            SetHealthStatuses();

            SetWidths();

            SetErrors(sprintTasks);
        }

        public static int GetSprintDaysWorked(Sprint sprint, JiraPolicy policy)
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (now <= sprint.EndDate.AddDays(-1).ToOriginalTimeZone())
                return Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), now.AddDays(-1).Date, policy.MonthlyOptions);

            return Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), sprint.EndDate.ToOriginalTimeZone().AddDays(-1), policy.MonthlyOptions);
        }

        private void GetTasksCount(SprintTasks sprintTasks)
        {
            InProgressUnassignedCount = sprintTasks.InProgressTasks.Count(tasks => tasks.Assignee == null);
            OpenUnassignedCount = sprintTasks.OpenTasks.Count(tasks => tasks.Assignee == null);
            InProgressTasksCount = sprintTasks.InProgressTasks.Count;
            OpenTasksCount = sprintTasks.OpenTasks.Count;
        }

        private void SetDates(JiraOptions options)
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
            if (Policy.IsThisMonthOverriden == true && Policy.CurrentOverride.AllocatedHoursPerDay > 0)
                AllocatedHoursPerDay = Policy.CurrentOverride.AllocatedHoursPerDay;
            else
                AllocatedHoursPerDay = Policy.AllocatedHoursPerDay;
        }

        private void SetAllocatedTimePerMonth()
        {
            if (Policy.IsThisMonthOverriden == true && Policy.CurrentOverride.AllocatedHoursPerMonth > 0)
                AllocatedHoursPerMonth = Policy.CurrentOverride.AllocatedHoursPerMonth;
            else
                AllocatedHoursPerMonth = Policy.AllocatedHoursPerMonth;
        }

        private void SetTimeWorked()
        {
            TotalTimeSeconds = Authors.Sum(a => a.TimeSpent);
            MonthHoursWorked = (double)Authors.Sum(a => a.TimeSpentCurrentMonthSeconds) / 3600;   //TODO: set month time worked in seconds 
            if (Sprint != null)
                SprintHoursWorked = (double)Authors.Sum(a => a.TimeSpentCurrentSprintSeconds) / 3600; //TODO: set sprint time worked in seconds
        }

        private void SetAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            SetTotalAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
            SetAuthorsAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
        }

        private void SetTotalAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            if (monthWorkedDays > 0)
                MonthWorkedPerDay = (MonthHoursWorked * 3600) / monthWorkedDays;
            if (sprintWorkedDays > 0)
                SprintWorkedPerDay = (SprintHoursWorked * 3600) / sprintWorkedDays;
            if (reportWorkingDays > 0)
                AverageTimeWorked = (double)TotalTimeSeconds / reportWorkingDays;
        }

        private void SetAverageRemainingTimePerDay(int monthRemainingDays, int sprintRemainingDays)
        {
            if (monthRemainingDays == 0)
                RemainingMonthAverage = 0;
            else
                RemainingMonthAverage = (RemainingMonthHours * 3600) / monthRemainingDays;

            if (sprintRemainingDays == 0)
                SprintTasksTimeLeftPerDay = 0;
            else
                SprintTasksTimeLeftPerDay = SprintTasksTimeLeftSeconds / sprintRemainingDays;
        }

        private void SetMonthEstimatedValue(int monthWorkingDays)
        {
            int hours = 0;
            if (monthWorkingDays > 0)
                hours = Policy.AllocatedHoursPerMonth / monthWorkingDays;
            MonthAverageEstimated = hours * 3600;
        }

        private int GetTimesheetTotalEstimate(Timesheet timesheet)
        {
            var timesheetService = new TimesheetService();
            if (timesheet == null)
                return 0;
            else
                return timesheetService.GetTotalOriginalEstimate(timesheet);
        }

        private void SetAuthorsAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            foreach (var author in Authors)
                AuthorHelpers.SetAuthorAverageWorkPerDay(author, monthWorkedDays, sprintWorkedDays, reportWorkingDays);
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

        private void SetRemainingMonthlyHours()
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

        public static int GetWorkingDays(DateTime startDate, DateTime endDate, List<Month> currentOverrides)
        {
            DateTime dateIterator = startDate.Date;
            int days = 0;
            while (dateIterator <= endDate)
            {
                if (dateIterator.DayOfWeek != DayOfWeek.Saturday && dateIterator.DayOfWeek != DayOfWeek.Sunday && MonthlyOptionsHelpers.SearchDateInOverrides(currentOverrides, dateIterator) == false)
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

        private void SetHealth(int sprintWorkedDays)
        {
            var healthInspector = new HealthInspector(Policy);
            WorkedDaysHealth = healthInspector.GetWorkedDaysHealth(AllocatedHoursPerDay * GetWorkingDays(FromDate, ToDate.AddDays(-1), Policy.MonthlyOptions), TotalTimeHours);
            if (Sprint == null)
            {
                DayHealth = Health.None;
                SprintHealth = Health.None;
            }
            else
            {
                DayHealth = healthInspector.GetDayHealth(AllocatedHoursPerDay, SprintHourRate);
                SprintHealth = healthInspector.GetSprintHealth(sprintWorkedDays, AllocatedHoursPerDay, SprintHoursWorked);
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
            SetAuthorsNotConfirmed();
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
            if (ConfirmationErrors != null)
                errors = errors.Concat(ConfirmationErrors).ToList();
            Errors = errors;
        }

        private void SetAuthorsWithErrors()
        {
            AuthorsWithErrors = new List<Author>();
            AuthorsWithErrors = Authors.FindAll(a => a.Errors != null && a.Errors.Count > 0).ToList();
        }

        private void SetAuthorsNotConfirmed()
        {
            if (!Policy.GeneratedProperties.IsFinalDraft || Policy.AdvancedOptions.NoIndividualDraft)
                return;

            AuthorsNotConfirmed = new List<Author>();
            ConfirmationErrors = new List<Error>();
            var notConfirmed = Policy.GeneratedProperties.IndividualDrafts.Where(d => !d.Confirmed).ToList();
            foreach (var author in Authors)
            {
                var notConfirmedAuthor = notConfirmed.Exists(a => a.Username == author.Username);
                if (notConfirmedAuthor)
                {
                    AuthorsNotConfirmed.Add(author);
                    ConfirmationErrors.Add(new Error(ErrorType.NotConfirmed));
                }
            }
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
            var max = new List<double>();
            foreach (var author in Authors)
            {
                var maxFromAuthor = AuthorHelpers.GetAuthorMaxAverage(author);
                max.Add(maxFromAuthor);
            }
            double maxHours = (double)max.Max() / 3600;
            return MathHelpers.RoundToNextEvenInteger(maxHours);
        }

        private int GetStatusMax()
        {
            var sprintMax = GetSprintMax();
            var monthMax = GetMonthMax();
            var statusMax = Math.Max(sprintMax, monthMax);
            return MathHelpers.RoundToNextEvenInteger(statusMax);
        }

        private double GetSprintMax()
        {
            var max = new List<double>();
            max.Add(SprintWorkedPerDay);
            max.Add(AverageTimeWorked);
            max.Add(SprintAverageEstimate);
            max.Add(SprintTasksTimeLeftPerDay);
            var maxHours = max.Max() / 3600;
            return maxHours;
        }

        private double GetMonthMax()
        {
            var max = new List<double>();
            max.Add(MonthAverageEstimated);
            max.Add(MonthWorkedPerDay);
            max.Add(RemainingMonthAverage);
            var maxHours = max.Max() / 3600;
            return maxHours;
        }

        private void SetWidths()
        {
            var widthHelper = new WidthHelpers(ChartMaxBarWidth);
            var workSummaryMax = GetWorkSummaryMax();
            widthHelper.SetWorkSummaryChartWidths(Authors, workSummaryMax);

            var statusMax = GetStatusMax();
            GetStatusValues();
            widthHelper.SetStatusChartWidths(statusMax, SprintWidths, MonthWidths);
        }

        private void GetStatusValues()
        {
            GetStatusSprintValues();
            GetStatusMonthValues();
        }

        private void GetStatusSprintValues()
        {
            SprintWidths = new StatusChartWidths();
            SprintWidths.DaySeconds = AverageTimeWorked;
            SprintWidths.EstimatedSeconds = SprintAverageEstimate;
            SprintWidths.DoneSeconds = SprintWorkedPerDay;
            SprintWidths.RemainingSeconds = SprintTasksTimeLeftPerDay;
        }

        private void GetStatusMonthValues()
        {
            MonthWidths = new StatusChartWidths();
            MonthWidths.EstimatedSeconds = MonthAverageEstimated;
            MonthWidths.DoneSeconds = MonthWorkedPerDay;
            MonthWidths.RemainingSeconds = RemainingMonthAverage;
        }
    }
}