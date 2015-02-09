using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class SummaryLoader
    {
        public List<JiraAuthor> _authors;
        public SprintTasks _sprintTasks;
        public List<JiraPullRequest> _pullRequests;
        public JiraPolicy _policy;
        public JiraOptions _options;
        public Sprint _sprint;
        public Summary _summary;

        public SummaryLoader(JiraPolicy policy, JiraOptions options, List<JiraAuthor> authors, SprintTasks sprintTasks, Sprint sprint, List<JiraPullRequest> pullRequests)
        {
            _authors = authors;
            _sprintTasks = sprintTasks;
            _pullRequests = pullRequests;
            _policy = policy;
            _options = options;
            _sprint = sprint;
        }

        public Summary LoadSummary()
        {
            _summary = new Summary();

            SetSummary();

            return _summary;
        }

        private void SetSummary()
        {
            _summary.Policy = _policy;
            _summary.Authors = _authors;
            _summary.CommitsCount = _authors.Where(a => a.Commits != null).Sum(a => a.Commits.Count);
            _summary.PullRequests = _pullRequests;
            _summary.Sprint = _sprint;
            _summary.UnrelatedPullRequests = _pullRequests.FindAll(p => p.TaskSynced == false);
            _summary.UnassignedTasksCount = _sprintTasks.UnassignedTasks.Count(t => t.IsSubtask == false);
            _summary.WorkingDays = new WorkingDaysInfo(_sprint, _policy, _options);
            _summary.Timing = new TimingDetailed();

            SetDates(_options);
            SetReportDate();
            SetAllocatedTime();

            SetTimeWorked();
            SetMonthEstimatedValue(_summary.WorkingDays.MonthWorkingDays);
            SetAverageTimeWorkedPerDay(_summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.ReportWorkingDays);
            SetSummaryTasksTimeLeft(_sprintTasks);
            _summary.Timing.TotalRemainingSeconds = GetSprintTimeLeftSeconds();
            SetRemainingMonthlyHours();
            SetAverageRemainingTimePerDay(_summary.WorkingDays.MonthWorkingDays, _summary.WorkingDays.SprintWorkingDaysLeft);

            GetTasksCount(_sprintTasks);

            _summary.Timing.OpenUnassignedTasksSecondsLeft = TasksService.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.OpenTasks, null);
            _summary.Timing.InProgressUnassignedTasksSecondsLeft = TasksService.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.InProgressTasks, null);
            _summary.Timing.UnassignedTasksSecondsLeft = _summary.Timing.OpenUnassignedTasksSecondsLeft + _summary.Timing.InProgressUnassignedTasksSecondsLeft;

            SetHourRates(_summary.WorkingDays.MonthWorkingDaysLeft, _summary.WorkingDays.SprintWorkingDaysLeft);

            SetHealthColors();
            SetVariances(_summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.MonthWorkingDays);

            SetHealth(_summary.WorkingDays.SprintWorkedDays);
            SetHealthStatuses();

            SetWidths();

            SetErrors(_sprintTasks);
        }

        private void SetReportDate()
        {
            if ((_options.ToDate - _options.FromDate).Days <= 1)
                _summary.ReportDate = _options.FromDate.DayOfWeek.ToString();
            else
                _summary.ReportDate = _options.FromDate.ToString("m") + " - " + _options.ToDate.AddDays(-1).ToString("m");
        }

        private void GetTasksCount(SprintTasks sprintTasks)
        {
            _summary.InProgressUnassignedCount = sprintTasks.InProgressTasks.Count(tasks => tasks.Assignee == null);
            _summary.OpenUnassignedCount = sprintTasks.OpenTasks.Count(tasks => tasks.Assignee == null);
            _summary.InProgressTasksCount = sprintTasks.InProgressTasks.Count;
            _summary.OpenTasksCount = sprintTasks.OpenTasks.Count;
        }

        private void SetDates(JiraOptions options)
        {
            _summary.FromDate = options.FromDate;
            _summary.ToDate = options.ToDate;
        }

        private void SetAllocatedTime()
        {
            SetAllocatedTimePerDay();
            SetAllocatedTimePerMonth();
        }

        private void SetAllocatedTimePerDay()
        {
            if (_policy.IsThisMonthOverriden && _policy.CurrentOverride.AllocatedHoursPerDay > 0)
                _summary.Timing.AllocatedHoursPerDay = _policy.CurrentOverride.AllocatedHoursPerDay;
            else
                _summary.Timing.AllocatedHoursPerDay = _policy.AllocatedHoursPerDay;
        }

        private void SetAllocatedTimePerMonth()
        {
            if (_policy.IsThisMonthOverriden && _policy.CurrentOverride.AllocatedHoursPerMonth > 0)
                _summary.Timing.AllocatedHoursPerMonth = _policy.CurrentOverride.AllocatedHoursPerMonth;
            else
                _summary.Timing.AllocatedHoursPerMonth = _policy.AllocatedHoursPerMonth;
        }

        private void SetTimeWorked()
        {
            _summary.Timing.TotalTimeSeconds = _summary.Authors.Sum(a => a.Timing.TotalTimeSeconds);
            _summary.Timing.MonthHoursWorked = (double)_summary.Authors.Sum(a => a.Timing.MonthSecondsWorked) / 3600;  
            if (_sprint != null)
                _summary.Timing.SprintHoursWorked = (double)_summary.Authors.Sum(a => a.Timing.SprintSecondsWorked) / 3600; 
        }

        private void SetAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            SetTotalAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
            SetAuthorsAverageTimeWorkedPerDay(monthWorkedDays, sprintWorkedDays, reportWorkingDays);
        }

        private void SetTotalAverageTimeWorkedPerDay(int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            if (monthWorkedDays > 0)
                _summary.Timing.AverageWorkedMonth = (_summary.Timing.MonthHoursWorked * 3600) / monthWorkedDays;
            if (sprintWorkedDays > 0)
                _summary.Timing.AverageWorkedSprint = (_summary.Timing.SprintHoursWorked * 3600) / sprintWorkedDays;
            if (reportWorkingDays > 0)
                _summary.Timing.AverageWorked = (double)_summary.Timing.TotalTimeSeconds / reportWorkingDays;
        }

        private void SetAverageRemainingTimePerDay(int monthRemainingDays, int sprintRemainingDays)
        {
            if (monthRemainingDays == 0)
                _summary.Timing.RemainingMonthAverage = 0;
            else
                _summary.Timing.RemainingMonthAverage = (_summary.Timing.RemainingMonthHours * 3600) / monthRemainingDays;

            if (sprintRemainingDays == 0)
                _summary.Timing.SprintTasksTimeLeftPerDay = 0;
            else
                _summary.Timing.SprintTasksTimeLeftPerDay = _summary.Timing.TotalRemainingSeconds / sprintRemainingDays;
        }

        private void SetMonthEstimatedValue(int monthWorkingDays)
        {
            int hours = 0;
            if (monthWorkingDays > 0)
                hours = _policy.AllocatedHoursPerMonth / monthWorkingDays;
            _summary.Timing.MonthAverageEstimated = hours * 3600;
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
            foreach (var author in _summary.Authors)
                AuthorHelpers.SetAuthorAverageWorkPerDay(author, monthWorkedDays, sprintWorkedDays, reportWorkingDays);
        }

        private void SetSummaryTasksTimeLeft(SprintTasks tasks)
        {
            _summary.Timing.InProgressTasksTimeLeftSeconds = 0;
            _summary.Timing.OpenTasksTimeLeftSeconds = 0;

            if (tasks.InProgressTasks != null)
                _summary.Timing.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.InProgressTasks);
            if (tasks.OpenTasks != null)
                _summary.Timing.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(tasks.OpenTasks);
        }

        private void SetRemainingMonthlyHours()
        {
            if (_summary.Timing.AllocatedHoursPerMonth > 0)
                _summary.Timing.RemainingMonthHours = _summary.Timing.AllocatedHoursPerMonth - _summary.Timing.MonthHoursWorked;
        }

        private void SetHourRates(int monthWorkingDaysLeft, int sprintWorkingDaysLeft)
        {
            _summary.Timing.HourRateToCompleteMonth = _summary.Timing.RemainingMonthHours / monthWorkingDaysLeft;
            if (sprintWorkingDaysLeft > 0)
                _summary.Timing.HourRateToCompleteSprint = (double)_summary.Timing.TotalRemainingHours / sprintWorkingDaysLeft;
        }

        private int GetSprintTimeLeftSeconds()
        {
            return _summary.Timing.OpenTasksTimeLeftSeconds + _summary.Timing.InProgressTasksTimeLeftSeconds;
        }

        private void SetHealthColors()
        {
            _summary.HealthColors = new Dictionary<Health, string>();
            _summary.HealthColors.Add(Health.Bad, "#FFE7E7");
            _summary.HealthColors.Add(Health.Weak, "#FFD");
            _summary.HealthColors.Add(Health.Good, "#DDFADE");
            _summary.HealthColors.Add(Health.None, "#FFFFFF");
        }

        private void SetHealth(int sprintWorkedDays)
        {
            var healthInspector = new HealthInspector(_policy);
            _summary.WorkedDaysHealth = healthInspector.GetWorkedDaysHealth(_summary.Timing.AllocatedHoursPerDay * SummaryHelpers.GetWorkingDays(_options.FromDate, _options.ToDate.AddDays(-1), _policy.MonthlyOptions), _summary.Timing.TotalTimeHours);
            if (_sprint == null)
            {
                _summary.DayHealth = Health.None;
                _summary.SprintHealth = Health.None;
            }
            else
            {
                _summary.DayHealth = healthInspector.GetDayHealth(_summary.Timing.AllocatedHoursPerDay, _summary.Timing.HourRateToCompleteSprint);
                _summary.SprintHealth = healthInspector.GetSprintHealth(sprintWorkedDays, _summary.Timing.AllocatedHoursPerDay, _summary.Timing.SprintHoursWorked);
            }


            _summary.MonthHealth = healthInspector.GetMonthHealth(_summary.Timing.AllocatedHoursPerMonth, _summary.Timing.MonthHoursWorked);
        }

        public void SetSprintVariance(int sprintWorkedDays)
        {
            _summary.SprintHourRateVariance = MathHelpers.GetVariance(_summary.Timing.AllocatedHoursPerDay * sprintWorkedDays, _summary.Timing.SprintHoursWorked);
        }

        public void SetMonthVariance(int monthWorkedDays, int monthWorkingDays)
        {
            var workedPerDay = _summary.Timing.MonthHoursWorked / monthWorkedDays;
            var averageFromAllocatedHours = _summary.Timing.AllocatedHoursPerMonth / monthWorkingDays;
            _summary.MonthHourRateVariance = MathHelpers.GetVariance(workedPerDay, averageFromAllocatedHours);
        }

        public void SetVariances(int sprintWorkedDays, int monthWorkedDays, int monthWorkingDays)
        {
            if (sprintWorkedDays > 0)
                SetSprintVariance(sprintWorkedDays);
            if (_summary.Timing.AllocatedHoursPerMonth > 0)
                SetMonthVariance(monthWorkedDays, monthWorkingDays);
        }

        private void SetHealthStatuses()
        {
            _summary.SprintStatus = HealthInspector.GetSprintStatus(_summary.SprintHealth, _summary.SprintHourRateVariance);
            _summary.MonthStatus = HealthInspector.GetMonthStatus(_summary.MonthHealth, _summary.MonthHourRateVariance);
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
            if (_summary.AuthorsWithErrors != null)
                errors = _summary.AuthorsWithErrors.SelectMany(e => e.Errors).ToList();
            if (_summary.CompletedWithEstimateErrors != null)
                errors = errors.Concat(_summary.CompletedWithEstimateErrors).ToList();
            if (_summary.CompletedWithNoWorkErrors != null)
                errors = errors.Concat(_summary.CompletedWithNoWorkErrors).ToList();
            if (_summary.UnassignedErrors != null)
                errors = errors.Concat(_summary.UnassignedErrors).ToList();
            if (_summary.ConfirmationErrors != null)
                errors = errors.Concat(_summary.ConfirmationErrors).ToList();
            _summary.Errors = errors;
        }

        private void SetAuthorsWithErrors()
        {
            _summary.AuthorsWithErrors = new List<JiraAuthor>();
            _summary.AuthorsWithErrors = _summary.Authors.FindAll(a => a.Errors != null && a.Errors.Count > 0).ToList();
        }

        private void SetAuthorsNotConfirmed()
        {
            if (!_policy.GeneratedProperties.IsFinalDraft || _policy.AdvancedOptions.NoIndividualDraft)
                return;

            _summary.AuthorsNotConfirmed = new List<JiraAuthor>();
            _summary.ConfirmationErrors = new List<Error>();
            var notConfirmed = _policy.GeneratedProperties.IndividualDrafts.Where(d => !d.Confirmed).ToList();
            foreach (var author in _summary.Authors)
            {
                var notConfirmedAuthor = notConfirmed.Exists(a => a.Username == author.Username);
                if (notConfirmedAuthor)
                {
                    _summary.AuthorsNotConfirmed.Add(author);
                    _summary.ConfirmationErrors.Add(new Error(ErrorType.NotConfirmed));
                }
            }
        }

        private void GetCompletedTasksErrors(SprintTasks tasks)
        {
            _summary.CompletedWithEstimateErrors = new List<Error>();
            _summary.CompletedWithNoWorkErrors = new List<Error>();
            if (tasks.CompletedTasks != null)
                foreach (var completedTasks in tasks.CompletedTasks.Values)
                {
                    var tasksWithErrors = completedTasks.Where(t => t.ErrorsCount > 0);
                    var errorsWithEstimate = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasRemaining)).ToList();
                    var errorsWithNoTimeSpent = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasNoTimeSpent)).ToList();
                    _summary.CompletedWithEstimateErrors = _summary.CompletedWithEstimateErrors.Concat(errorsWithEstimate).ToList();
                    _summary.CompletedWithNoWorkErrors = _summary.CompletedWithNoWorkErrors.Concat(errorsWithNoTimeSpent).ToList();
                }
        }

        private void GetUnassignedErrors(SprintTasks tasks)
        {
            _summary.UnassignedErrors = new List<Error>();
            if (tasks.UnassignedTasks != null && tasks.UnassignedTasks.Count > 0)
            {
                var errors = new List<Error>();
                errors = tasks.UnassignedTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
                _summary.UnassignedErrors = _summary.UnassignedErrors.Concat(errors).ToList();
            }
        }

        private int GetWorkSummaryMax()
        {
            var max = new List<double>();
            foreach (var author in _summary.Authors)
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
            max.Add(_summary.Timing.AverageWorkedSprint);
            max.Add(_summary.Timing.AverageWorked);
            max.Add(_summary.Timing.SprintAverageEstimate);
            max.Add(_summary.Timing.SprintTasksTimeLeftPerDay);
            var maxHours = max.Max() / 3600;
            return maxHours;
        }

        private double GetMonthMax()
        {
            var max = new List<double>();
            max.Add(_summary.Timing.MonthAverageEstimated);
            max.Add(_summary.Timing.AverageWorkedMonth);
            max.Add(_summary.Timing.RemainingMonthAverage);
            var maxHours = max.Max() / 3600;
            return maxHours;
        }

        private void SetWidths()
        {
            var widthHelper = new WidthHelpers(_summary.ChartMaxBarWidth);
            var workSummaryMax = GetWorkSummaryMax();
            widthHelper.SetWorkSummaryChartWidths(_summary.Authors, workSummaryMax);

            var statusMax = GetStatusMax();
            GetStatusValues();
            widthHelper.SetStatusChartWidths(statusMax, _summary.SprintWidths, _summary.MonthWidths);
        }

        private void GetStatusValues()
        {
            GetStatusSprintValues();
            GetStatusMonthValues();
        }

        private void GetStatusSprintValues()
        {
            _summary.SprintWidths = new StatusChartWidths();
            _summary.SprintWidths.DaySeconds = _summary.Timing.AverageWorked;
            _summary.SprintWidths.EstimatedSeconds = _summary.Timing.SprintAverageEstimate;
            _summary.SprintWidths.DoneSeconds = _summary.Timing.AverageWorkedSprint;
            _summary.SprintWidths.RemainingSeconds = _summary.Timing.SprintTasksTimeLeftPerDay;
        }

        private void GetStatusMonthValues()
        {
            _summary.MonthWidths = new StatusChartWidths();
            _summary.MonthWidths.EstimatedSeconds = _summary.Timing.MonthAverageEstimated;
            _summary.MonthWidths.DoneSeconds = _summary.Timing.AverageWorkedMonth;
            _summary.MonthWidths.RemainingSeconds = _summary.Timing.RemainingMonthAverage;
        }

    }
}
