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
using Equilobe.DailyReport.Models.Policy;

namespace JiraReporter.Services
{
    class SummaryLoader
    {
        public List<JiraAuthor> _authors { get { return _report.Authors; } }
        public SprintTasks _sprintTasks { get { return _report.ReportTasks; } }
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
            if (_report.HasSprint)
            {
                _summary.HasSprint = true;
                _summary.UnassignedTasksCount = _sprintTasks.UnassignedTasks.Count(t => t.IsSubtask == false);
            }
            _summary.WorkingDays = LoadWorkingDaysInfo();
            _summary.Timing = new TimingDetailed();
            _summary.IsFinalDraft = _report.IsFinalDraft;

            SetDates();
            SetReportDate();
            SetAllocatedTime();

            SetTimeWorked();
            SetMonthEstimatedValue();
            SetSprintEstimatedValue();
            SetAverageTimeWorkedPerDay();
            SetSummaryTasksTimeLeft();
            SetRemainingMonthlyHours();
            SetAverageRemainingTimePerDay();

            GetTasksCount();

            SetUnassignedTimming();

            SetHealthColors();
            SetVariances();

            SetHealth();
            SetHealthStatuses();

            SetMaxValues();
            SetWidths();
            CheckSummaryCharts();
            SetGuidelines();

            SetErrors();
        }

        private void CheckSummaryCharts()
        {
            if (_summary.Timing.AverageWorkedMonth > 0 || _policy.AllocatedHoursPerMonth > 0)
                _summary.HasMonth = true;
            if (_summary.HasMonth == true || _summary.Sprint != null)
                _summary.HasStatus = true;
            if (_summary.Authors.Exists(a => !a.IsEmpty))
                _summary.HasWorkSummary = true;
        }

        private void SetUnassignedTimming()
        {
            if (!_report.HasSprint)
                return;

            var unassignedTasksHoursLeft = ((double)_summary.Timing.UnassignedTasksSecondsLeft / 3600);

            _summary.Timing.OpenUnassignedTasksSecondsLeft = TaskLoader.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.OpenTasks, null);
            _summary.Timing.OpenUnassignedTasksTimeLeftString = _summary.Timing.OpenUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.InProgressUnassignedTasksSecondsLeft = TaskLoader.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.InProgressTasks, null);
            _summary.Timing.InProgressUnassignedTasksTimeLeftString = _summary.Timing.InProgressUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.UnassignedTasksSecondsLeft = _summary.Timing.OpenUnassignedTasksSecondsLeft + _summary.Timing.InProgressUnassignedTasksSecondsLeft;
            _summary.Timing.UnassignedTasksHoursAverageLeft = _summary.WorkingDays.SprintWorkingDaysLeft != 0 ? unassignedTasksHoursLeft / _summary.WorkingDays.SprintWorkingDaysLeft : unassignedTasksHoursLeft;
            _summary.Timing.UnassignedTasksTimeLeftString = _summary.Timing.UnassignedTasksHoursAverageLeft.RoundDoubleOneDecimal();
        }

        private void SetReportDate()
        {
            if ((_options.ToDate - _options.FromDate).Days <= 1)
                _summary.ReportDate = _options.FromDate.DayOfWeek.ToString();
            else
                _summary.ReportDate = _options.FromDate.ToString("m") + " - " + _options.ToDate.AddDays(-1).ToString("m");
        }

        private void GetTasksCount()
        {
            if (!_report.HasSprint)
                return;

            _summary.InProgressUnassignedCount = _report.ReportTasks.InProgressTasks.Count(tasks => tasks.Assignee == null);
            _summary.OpenUnassignedCount = _report.ReportTasks.OpenTasks.Count(tasks => tasks.Assignee == null);
            _summary.InProgressTasksCount = _report.ReportTasks.InProgressTasks.Count;
            _summary.OpenTasksCount = _report.ReportTasks.OpenTasks.Count;
        }

        private void SetDates()
        {
            _summary.FromDate = _options.FromDate;
            _summary.ToDate = _options.ToDate;
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
            if (_report.HasSprint)
                _summary.Timing.SprintHoursWorked = (double)_summary.Authors.Sum(a => a.Timing.SprintSecondsWorked) / 3600;
        }

        private void SetAverageTimeWorkedPerDay()
        {
            SetTotalAverageTimeWorkedPerDay();
            SetAuthorsAverageTiming();
        }

        private void SetTotalAverageTimeWorkedPerDay()
        {
            if (_summary.WorkingDays.MonthWorkedDays > 0)
                _summary.Timing.AverageWorkedMonth = (_summary.Timing.MonthHoursWorked * 3600) / _summary.WorkingDays.MonthWorkedDays;
            if (_report.HasSprint)
                _summary.Timing.AverageWorkedSprint = (_summary.Timing.SprintHoursWorked * 3600) / _summary.WorkingDays.SprintWorkedDays;
            if (_summary.WorkingDays.ReportWorkingDays > 0)
                _summary.Timing.AverageWorked = (double)_summary.Timing.TotalTimeSeconds / _summary.WorkingDays.ReportWorkingDays;

            TimingHelpers.SetAverageWorkStringFormat(_summary.Timing);
        }

        private void SetAverageRemainingTimePerDay()
        {
            if (_summary.WorkingDays.MonthWorkingDaysLeft != 0)
                _summary.Timing.RemainingMonthAverage = (_summary.Timing.RemainingMonthHours * 3600) / _summary.WorkingDays.MonthWorkingDaysLeft;
            else
                _summary.Timing.RemainingMonthAverage = _summary.Timing.RemainingMonthHours;

            if (_summary.WorkingDays.SprintWorkingDaysLeft != 0)
                _summary.Timing.RemainingSprintAverage = _summary.Timing.TotalRemainingSeconds / _summary.WorkingDays.SprintWorkingDaysLeft;
            else
                _summary.Timing.RemainingSprintAverage = (double)(_summary.Timing.TotalRemainingSeconds / 3600);

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

        private void SetSprintEstimatedValue()
        {
            if (!_report.HasSprint)
                return;

            _summary.Timing.SprintAverageEstimate = _summary.Timing.AllocatedHoursPerDay * 3600;
            _summary.Timing.SprintAverageEstimateString = _summary.Timing.AllocatedHoursPerDay.RoundDoubleOneDecimal();
        }

        private void SetAuthorsAverageTiming()
        {
            foreach (var author in _summary.Authors)
            {
                AuthorHelpers.SetAuthorAverageWorkPerDay(author, _summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.ReportWorkingDays);
                AuthorHelpers.SetAuthorAverageRemainig(author, _summary.WorkingDays.SprintWorkingDaysLeft);
            }
        }

        private void SetSummaryTasksTimeLeft()
        {
            if (!_report.HasSprint)
                return;

            _summary.Timing.InProgressTasksTimeLeftSeconds = 0;
            _summary.Timing.OpenTasksTimeLeftSeconds = 0;

            if (_report.ReportTasks.InProgressTasks != null)
                _summary.Timing.InProgressTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_report.ReportTasks.InProgressTasks);
            if (_report.ReportTasks.OpenTasks != null)
                _summary.Timing.OpenTasksTimeLeftSeconds = IssueAdapter.GetTasksTimeLeftSeconds(_report.ReportTasks.OpenTasks);

            _summary.Timing.InProgressTasksTimeLeftString = _summary.Timing.InProgressTasksTimeLeftSeconds.SetTimeFormat8Hour();
            _summary.Timing.OpenTasksTimeLeftString = _summary.Timing.OpenTasksTimeLeftSeconds.SetTimeFormat8Hour();

            _summary.Timing.TotalRemainingSeconds = _summary.Timing.OpenTasksTimeLeftSeconds + _summary.Timing.InProgressTasksTimeLeftSeconds;
            _summary.Timing.TotalRemainingAverage = _summary.WorkingDays.SprintWorkingDaysLeft != 0 ? _summary.Timing.TotalRemainingHours / _summary.WorkingDays.SprintWorkingDaysLeft : _summary.Timing.TotalRemainingHours;
            _summary.Timing.TotalRemainingString = _summary.Timing.TotalRemainingAverage.RoundDoubleOneDecimal();
        }

        private void SetRemainingMonthlyHours()
        {
            if (_summary.Timing.AllocatedHoursPerMonth > 0)
                _summary.Timing.RemainingMonthHours = _summary.Timing.AllocatedHoursPerMonth - _summary.Timing.MonthHoursWorked;
        }

        private void SetHealthColors()
        {
            _summary.HealthColors = new Dictionary<Health, string>();
            _summary.HealthColors.Add(Health.Bad, "#FFE7E7");
            _summary.HealthColors.Add(Health.Weak, "#FFD");
            _summary.HealthColors.Add(Health.Good, "#DDFADE");
            _summary.HealthColors.Add(Health.None, "#FFFFFF");
        }

        private void SetHealth()
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
                _summary.DayHealth = healthInspector.GetDayHealth(_summary.Timing.AllocatedHoursPerDay, _summary.Timing.RemainingSprintAverage);
                _summary.SprintHealth = healthInspector.GetSprintHealth(_summary.WorkingDays.SprintWorkedDays, _summary.Timing.AllocatedHoursPerDay, _summary.Timing.SprintHoursWorked);
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

        public void SetVariances()
        {
            if (_report.HasSprint)
                SetSprintVariance(_summary.WorkingDays.SprintWorkedDays);
            if (_summary.Timing.AllocatedHoursPerMonth > 0)
                SetMonthVariance(_summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.MonthWorkingDays);
        }

        private void SetHealthStatuses()
        {
            if (_report.HasSprint)
                _summary.SprintStatus = HealthInspector.GetSprintStatus(_summary.SprintHealth, _summary.SprintHourRateVariance);
            _summary.MonthStatus = HealthInspector.GetMonthStatus(_summary.MonthHealth, _summary.MonthHourRateVariance);
        }

        private void SetErrors()
        {
            SetAuthorsWithErrors();
            SetAuthorsNotConfirmed();
            GetCompletedTasksErrors();
            GetUnassignedErrors();
            GetAllErrors();
        }

        private void GetAllErrors()
        {
            var errors = new List<Error>();
            if (_summary.CompletedWithNoWorkErrors != null)
                errors = errors.Concat(_summary.CompletedWithNoWorkErrors).ToList();
            if (_summary.ConfirmationErrors != null)
                errors = errors.Concat(_summary.ConfirmationErrors).ToList();

            if (_report.HasSprint)
            {
                if (_summary.AuthorsWithErrors != null)
                    errors = errors.Concat(_summary.AuthorsWithErrors.SelectMany(e => e.Errors)).ToList();
                if (_summary.CompletedWithEstimateErrors != null)
                    errors = errors.Concat(_summary.CompletedWithEstimateErrors).ToList();
                if (_summary.UnassignedErrors != null)
                    errors = errors.Concat(_summary.UnassignedErrors).ToList();
            }

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
            var notConfirmed = _report.Settings.IndividualDraftConfirmations
                .Where(d=>d.ReportDate == _report.ToDate)
                .Where(d => d.LastDateConfirmed == null || d.LastDateConfirmed.Value.Date != _report.ToDate)
                .ToList();

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

        private void GetCompletedTasksErrors()
        {
            _summary.CompletedWithEstimateErrors = new List<Error>();
            _summary.CompletedWithNoWorkErrors = new List<Error>();
            if (_report.ReportTasks.CompletedTasks != null)
                foreach (var completedTasks in _report.ReportTasks.CompletedTasks.Values)
                {
                    var tasksWithErrors = completedTasks.Where(t => t.ErrorsCount > 0);
                    var errorsWithEstimate = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasRemaining)).ToList();
                    var errorsWithNoTimeSpent = tasksWithErrors.SelectMany(e => e.Errors.Where(er => er.Type == ErrorType.HasNoTimeSpent)).ToList();
                    _summary.CompletedWithEstimateErrors = _summary.CompletedWithEstimateErrors.Concat(errorsWithEstimate).ToList();
                    _summary.CompletedWithNoWorkErrors = _summary.CompletedWithNoWorkErrors.Concat(errorsWithNoTimeSpent).ToList();
                }
        }

        private void GetUnassignedErrors()
        {
            _summary.UnassignedErrors = new List<Error>();
            if (_report.ReportTasks.UnassignedTasks != null && _report.ReportTasks.UnassignedTasks.Count > 0)
            {
                var errors = new List<Error>();
                errors = _report.ReportTasks.UnassignedTasks.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
                _summary.UnassignedErrors = _summary.UnassignedErrors.Concat(errors).ToList();
            }
        }

        private void SetWorkSummaryMax()
        {
            var max = new List<double>();
            foreach (var author in _summary.Authors)
            {
                var maxFromAuthor = AuthorHelpers.GetAuthorMaxAverage(author) / 3600;
                author.MaxHourValue = MathHelpers.RoundToNextEvenInteger(maxFromAuthor);
                max.Add(maxFromAuthor);
            }
            max.Add(_summary.Timing.UnassignedTasksHoursAverageLeft);
            double maxHours = (double)max.Max();
            _summary.WorkSummaryMaxValue = MathHelpers.RoundToNextEvenInteger(maxHours);
        }

        private void SetStatusMax()
        {
            var sprintMax = GetSprintMax();
            var monthMax = GetMonthMax();
            var statusMax = Math.Max(sprintMax, monthMax);
            _summary.StatusMaxValue = MathHelpers.RoundToNextEvenInteger(statusMax);
        }

        private void SetMaxValues()
        {
            SetWorkSummaryMax();
            SetStatusMax();
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
            var widthHelper = new SummaryWidthLoader(_summary.ChartMaxBarWidth);
            widthHelper.SetWorkSummaryChartWidths(_summary, _summary.WorkSummaryMaxValue, _report.IsIndividualDraft);

            GetStatusValues();
            widthHelper.SetStatusChartWidths(_summary.StatusMaxValue, _summary.SprintWidths, _summary.MonthWidths);
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
            var reportDate = _report.ToDate.AddDays(-1);
            workingDaysInfo.ReportWorkingDays = SummaryHelpers.GetWorkingDays(_options.FromDate, reportDate.AddDays(1), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkingDays = SummaryHelpers.GetWorkingDays(reportDate.StartOfMonth(), reportDate.EndOfMonth().AddDays(1), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkingDaysLeft = SummaryHelpers.GetWorkingDays(reportDate.AddDays(1), reportDate.EndOfMonth().AddDays(1), _policy.MonthlyOptions);
            workingDaysInfo.MonthWorkedDays = SummaryHelpers.GetWorkingDays(reportDate.StartOfMonth(), reportDate.AddDays(1), _policy.MonthlyOptions);
            if (_sprint != null)
            {
                var sprintEndDate = _sprint.EndDate.ToOriginalTimeZone(_report.OffsetFromUtc);
                var sprintStartDate = _sprint.StartDate.ToOriginalTimeZone(_report.OffsetFromUtc);
                workingDaysInfo.SprintWorkingDaysLeft = SummaryHelpers.GetWorkingDays(DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc), sprintEndDate.AddDays(1), _policy.MonthlyOptions);
                workingDaysInfo.SprintWorkingDays = SummaryHelpers.GetWorkingDays(sprintStartDate, sprintEndDate.AddDays(1), _policy.MonthlyOptions);
                workingDaysInfo.SprintWorkedDays = SummaryHelpers.GetSprintDaysWorked(_report);
            }
            return workingDaysInfo;
        }

        private void SetGuidelines()
        {
            if (_report.IsIndividualDraft)
                SetGuidelinesForAuthors();
            else
            {
                SetStatusGuidelines();
                SetWorkSummaryGuidelines();
            }
        }

        private void SetStatusGuidelines()
        {
            _summary.GuidelineInfoStatus = new SummaryGuidelineInfo();
            _summary.GuidelineInfoStatus.GuidelinesRate = GetGuidelinesRate(_summary.StatusMaxValue);
            _summary.GuidelineInfoStatus.GuidelinesCount = GetGuidelinesCount(_summary.StatusMaxValue, _summary.GuidelineInfoStatus.GuidelinesRate);
            _summary.GuidelineInfoStatus.GuidelineWidth = GetGuidelineWidth(_summary.StatusMaxValue, _summary.GuidelineInfoStatus.GuidelinesRate);
        }

        private void SetWorkSummaryGuidelines()
        {
            _summary.GuidelineInfoWorkSummary = new SummaryGuidelineInfo();
            _summary.GuidelineInfoWorkSummary.GuidelinesRate = GetGuidelinesRate(_summary.WorkSummaryMaxValue);
            _summary.GuidelineInfoWorkSummary.GuidelinesCount = GetGuidelinesCount(_summary.WorkSummaryMaxValue, _summary.GuidelineInfoWorkSummary.GuidelinesRate);
            _summary.GuidelineInfoWorkSummary.GuidelineWidth = GetGuidelineWidth(_summary.WorkSummaryMaxValue, _summary.GuidelineInfoWorkSummary.GuidelinesRate);
        }

        private void SetGuidelinesForAuthors()
        {
            foreach (var author in _summary.Authors)
            {
                author.GuidelineInfo = new SummaryGuidelineInfo();
                author.GuidelineInfo.GuidelinesRate = GetGuidelinesRate(author.MaxHourValue);
                author.GuidelineInfo.GuidelinesCount = GetGuidelinesCount(author.MaxHourValue, author.GuidelineInfo.GuidelinesRate);
                author.GuidelineInfo.GuidelineWidth = GetGuidelineWidth(author.MaxHourValue, author.GuidelineInfo.GuidelinesRate);
            }
        }

        private int GetGuidelinesRate(int maxValue)
        {
            var integer = 2;
            while (maxValue / integer >= _summary.GuidelinesOptimalNumber)
                integer = integer * 2;
            return integer;
        }

        private int GetGuidelinesCount(int maxValue, int guidelinesRate)
        {
            if (maxValue % guidelinesRate != 0)
                return maxValue / guidelinesRate + 1;

            return maxValue / guidelinesRate;
        }

        private double GetGuidelineWidth(int maxValue, int guidelinesRate)
        {
            return MathHelpers.RuleOfThree(_summary.ChartMaxBarWidth, maxValue, guidelinesRate);
        }

    }
}
