using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using JiraReporter.Helpers;
using JiraReporter.Model;
using JiraReporter.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Services
{
    class SummaryLoader
    {
        public List<JiraAuthor> _authors { get { return _report.Authors; } }
        public SprintTasks _sprintTasks { get { return _report.SprintTasks; } }
        public List<JiraPullRequest> _pullRequests { get { return _report.PullRequests; } }
        public JiraPolicy _policy { get { return _report.Policy; } }
        public JiraOptions _options { get { return _report.Options; } }
        public Sprint _sprint { get { return _report.Sprint; } }
        public Summary _summary;
        public JiraReport _report;

        public SummaryLoader(JiraReport report)
        {
            _report = report;
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
            _summary.WorkingDays = LoadWorkingDaysInfo();
            _summary.Timing = new TimingDetailed();
            _summary.IsFinalDraft = _report.IsFinalDraft;

            SetDates(_options);
            SetReportDate();
            SetAllocatedTime();

            SetTimeWorked();
            SetMonthEstimatedValue();
            SetSpringEstimatedValue();
            SetAverageTimeWorkedPerDay(_summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.ReportWorkingDays);
            SetSummaryTasksTimeLeft(_sprintTasks);
            SetRemainingMonthlyHours();
            SetAverageRemainingTimePerDay(_summary.WorkingDays.MonthWorkingDays, _summary.WorkingDays.SprintWorkingDaysLeft);

            GetTasksCount(_sprintTasks);

            SetUnassignedTimming();

            SetHourRates(_summary.WorkingDays.MonthWorkingDaysLeft, _summary.WorkingDays.SprintWorkingDaysLeft);

            SetHealthColors();
            SetVariances(_summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.MonthWorkingDays);

            SetHealth(_summary.WorkingDays.SprintWorkedDays);
            SetHealthStatuses();

            SetWidths();

            SetErrors(_sprintTasks);
        }

        private void SetUnassignedTimming()
        {
            _summary.Timing.OpenUnassignedTasksSecondsLeft = TasksService.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.OpenTasks, null);
            _summary.Timing.OpenUnassignedTasksTimeLeftString = _summary.Timing.OpenUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.InProgressUnassignedTasksSecondsLeft = TasksService.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.InProgressTasks, null);
            _summary.Timing.InProgressUnassignedTasksTimeLeftString = _summary.Timing.InProgressUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.UnassignedTasksSecondsLeft = _summary.Timing.OpenUnassignedTasksSecondsLeft + _summary.Timing.InProgressUnassignedTasksSecondsLeft;
            _summary.Timing.UnassignedTasksTimeLeftString = _summary.Timing.UnassignedTasksSecondsLeft.SetTimeFormat8Hour();
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

            TimingHelpers.SetAverageWorkStringFormat(_summary.Timing);
        }

        private void SetAverageRemainingTimePerDay(int monthRemainingDays, int sprintRemainingDays)
        {
            if (monthRemainingDays == 0)
                _summary.Timing.RemainingMonthAverage = 0;
            else
                _summary.Timing.RemainingMonthAverage = (_summary.Timing.RemainingMonthHours * 3600) / monthRemainingDays;

            if (sprintRemainingDays == 0)
                _summary.Timing.RemainingSprintAverage = 0;
            else
                _summary.Timing.RemainingSprintAverage = _summary.Timing.TotalRemainingSeconds / sprintRemainingDays;

            TimingHelpers.SetAverageRemainingStringFormat(_summary.Timing);
        }

        private void SetMonthEstimatedValue()
        {
            double hours = 0;
            if (_summary.WorkingDays.MonthWorkingDays > 0)
                hours = _policy.AllocatedHoursPerMonth / _summary.WorkingDays.MonthWorkingDays;
            _summary.Timing.MonthAverageEstimated = hours * 3600;
            _summary.Timing.MonthAverageEstimatedString = hours.RoundDoubleOneDecimal();
        }

        private void SetSpringEstimatedValue()
        {
            _summary.Timing.SprintAverageEstimate = _summary.Timing.AllocatedHoursPerDay * 3600;
            _summary.Timing.SprintAverageEstimateString = _summary.Timing.AllocatedHoursPerDay.RoundDoubleOneDecimal();
        }

        //private int GetTimesheetTotalEstimate(Timesheet timesheet)
        //{
        //    var timesheetService = new TimesheetService();
        //    if (timesheet == null)
        //        return 0;
        //    else
        //        return timesheetService.GetTotalOriginalEstimate(timesheet);
        //}

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

            _summary.Timing.InProgressTasksTimeLeftString = _summary.Timing.InProgressTasksTimeLeftSeconds.SetTimeFormat8Hour();
            _summary.Timing.OpenTasksTimeLeftString = _summary.Timing.OpenTasksTimeLeftSeconds.SetTimeFormat8Hour();

            _summary.Timing.TotalRemainingSeconds = _summary.Timing.OpenTasksTimeLeftSeconds + _summary.Timing.InProgressTasksTimeLeftSeconds;
            _summary.Timing.TotalRemainingString = _summary.Timing.TotalRemainingSeconds.SetTimeFormat8Hour();
        }

        private void SetRemainingMonthlyHours()
        {
            if (_summary.Timing.AllocatedHoursPerMonth > 0)
                _summary.Timing.RemainingMonthHours = _summary.Timing.AllocatedHoursPerMonth - _summary.Timing.MonthHoursWorked;
        }

        private void SetHourRates(int monthWorkingDaysLeft, int sprintWorkingDaysLeft)
        {
            _summary.Timing.HourRateToCompleteMonth = _summary.Timing.RemainingMonthHours / monthWorkingDaysLeft;
            _summary.Timing.HourRateToCompleteMonthString = ((int)_summary.Timing.HourRateToCompleteMonth * 3600).SetTimeFormat8Hour();
            if (sprintWorkingDaysLeft > 0)
                _summary.Timing.HourRateToCompleteSprint = (double)_summary.Timing.TotalRemainingHours / sprintWorkingDaysLeft;
            _summary.Timing.HourRateToCompleteSprintString = ((int)_summary.Timing.HourRateToCompleteSprint * 3600).SetTimeFormat8Hour();
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
            var healthInspector = new HealthInspector(_report);
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
            if (!_report.IsFinalDraft || _policy.AdvancedOptions.NoIndividualDraft)
                return;

            _summary.AuthorsNotConfirmed = new List<JiraAuthor>();
            _summary.ConfirmationErrors = new List<Error>();
            var notConfirmed = _report.IndividualDrafts.Where(d => !d.Confirmed).ToList();
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
            max.Add(_summary.Timing.RemainingSprintAverage);
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
            _summary.SprintWidths.RemainingSeconds = _summary.Timing.RemainingSprintAverage;
        }

        private void GetStatusMonthValues()
        {
            _summary.MonthWidths = new StatusChartWidths();
            _summary.MonthWidths.EstimatedSeconds = _summary.Timing.MonthAverageEstimated;
            _summary.MonthWidths.DoneSeconds = _summary.Timing.AverageWorkedMonth;
            _summary.MonthWidths.RemainingSeconds = _summary.Timing.RemainingMonthAverage;
        }

        private WorkingDaysInfo LoadWorkingDaysInfo()
        {
            var workingDaysInfo = new WorkingDaysInfo();
            var now = DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc);
            workingDaysInfo.ReportWorkingDays = SummaryHelpers.GetWorkingDays(_options.FromDate, _options.ToDate.AddDays(-1), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkingDays = SummaryHelpers.GetWorkingDays(now.StartOfMonth(), now.EndOfMonth(), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkingDaysLeft = SummaryHelpers.GetWorkingDays(now, now.EndOfMonth(), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkedDays = SummaryHelpers.GetWorkingDays(now.StartOfMonth(), now.AddDays(-1), _policy.MonthlyOptions);
            if (_sprint != null)
            {
                var sprintEndDate = _sprint.EndDate.ToOriginalTimeZone(_report.OffsetFromUtc);
                var sprintStartDate = _sprint.StartDate.ToOriginalTimeZone(_report.OffsetFromUtc);
                workingDaysInfo.SprintWorkingDaysLeft = SummaryHelpers.GetWorkingDays(DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc), sprintEndDate, _policy.MonthlyOptions);
                workingDaysInfo.SprintWorkingDays = SummaryHelpers.GetWorkingDays(sprintStartDate, sprintEndDate, _policy.MonthlyOptions);
                workingDaysInfo.SprintWorkedDays = SummaryHelpers.GetSprintDaysWorked(_report);
            }
            return workingDaysInfo;
        }

    }
}
