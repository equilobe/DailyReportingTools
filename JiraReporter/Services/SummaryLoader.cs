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
using System.Globalization;
using Equilobe.DailyReport.Models.Interfaces;

namespace JiraReporter.Services
{
    class SummaryLoader
    {
        public List<JiraAuthor> _authors { get { return _report.Authors; } }
        public ReportTasks _sprintTasks { get { return _report.ReportTasks; } }
        public List<JiraPullRequest> _pullRequests { get { return _report.PullRequests; } }
        public JiraPolicy _policy { get { return _report.Policy; } }
        public JiraOptions _options { get { return _report.Options; } }
        public Sprint _sprint { get { return _report.Sprint; } }
        public Summary _summary;
        public JiraReport _report;
        public IErrorService ErrorService { get; set; }

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
                _summary.UnassignedTasksCount = _sprintTasks.UnassignedTasksAll.Count(t => t.IsSubtask == false);
            }
            _summary.WorkingDays = LoadWorkingDaysInfo();
            _summary.Timing = new TimingDetailed();
            _summary.IsFinalDraft = _report.IsFinalDraft;
            _summary.MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_report.Options.FromDate.Month);

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

            // SetHealth();
            //  SetHealthStatuses();

            SetMaxValues();
            SetGuidelines();

            SetCharts();
            CheckSummaryCharts();

            SetStatuses();
            SetErrors();
        }

        private void SetStatuses()
        {
            SetMonthStatus();
            SetSprintStatus();
        }

        private void SetSprintStatus()
        {
            if (!_report.HasSprint)
                return;

            if (_summary.Sprint.State != "ACTIVE")
            {
                _summary.SprintStatus = _summary.Sprint.State;
                return;
            }

            if (_summary.WorkingDays.SprintWorkingDaysLeft == 0)
            {
                _summary.SprintStatus = "Expired";
                return;
            }

            var daysLeft = _summary.WorkingDays.SprintWorkingDaysLeft == 1 ? "day left" : "days left";
            _summary.SprintStatus = _summary.WorkingDays.SprintWorkingDaysLeft + " " + daysLeft;
        }

        private void SetMonthStatus()
        {
            if (_summary.WorkingDays.MonthWorkingDaysLeft == 0 || DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc).Month != _report.Options.ToDate.AddDays(-1).Month)
            {
                _summary.MonthStatus = "Finished";
                return;
            }

            var daysLeft = _summary.WorkingDays.MonthWorkingDaysLeft == 1 ? "day left" : "days left";
            _summary.MonthStatus = _summary.WorkingDays.MonthWorkingDaysLeft + " " + daysLeft;
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

            _summary.Timing.OpenUnassignedTasksSecondsLeft = TaskLoader.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.OpenTasks, null);
            _summary.Timing.OpenUnassignedTasksTimeLeftString = _summary.Timing.OpenUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.InProgressUnassignedTasksSecondsLeft = TaskLoader.GetTimeLeftForSpecificAuthorTasks(_sprintTasks.InProgressTasks, null);
            _summary.Timing.InProgressUnassignedTasksTimeLeftString = _summary.Timing.InProgressUnassignedTasksSecondsLeft.SetTimeFormat8Hour();
            _summary.Timing.UnassignedTasksSecondsLeft = _summary.Timing.OpenUnassignedTasksSecondsLeft + _summary.Timing.InProgressUnassignedTasksSecondsLeft;

            var unassignedTasksHoursLeft = ((double)_summary.Timing.UnassignedTasksSecondsLeft / 3600);
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
            else
            {
                _summary.Timing.Last7DaySecondsWorked = _summary.Authors.Sum(a => a.Timing.Last7DaySecondsWorked);
            }
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
            else
                _summary.Timing.AverageWorkedLast7Days = (double)_summary.Timing.Last7DaySecondsWorked / 7;

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
                _summary.Timing.RemainingSprintAverage = (double)_summary.Timing.TotalRemainingSeconds;

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
                AuthorHelpers.SetAuthorAverageWorkPerDay(author, _summary.WorkingDays.MonthWorkedDays, _summary.WorkingDays.SprintWorkedDays, _summary.WorkingDays.ReportWorkingDays, _report.HasSprint);
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
            _summary.Timing.TotalRemainingAverage = _summary.WorkingDays.SprintWorkingDaysLeft != 0 ? _summary.Timing.TotalRemainingSeconds / _summary.WorkingDays.SprintWorkingDaysLeft : _summary.Timing.TotalRemainingSeconds;
            _summary.Timing.TotalRemainingHours = _summary.Timing.TotalRemainingSeconds / 3600;
            _summary.Timing.TotalRemainingString = (_summary.Timing.TotalRemainingAverage / 3600).RoundDoubleOneDecimal();
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
            _summary.WorkedDaysHealth = healthInspector.GetWorkedDaysHealth(_summary.Timing.AllocatedHoursPerDay * SummaryHelpers.GetWorkingDays(_options.FromDate, _options.ToDate.AddDays(-1), _report.WorkingDaysContext), _summary.Timing.TotalTimeHours);
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
                _summary.SprintHealthStatus = HealthInspector.GetSprintStatus(_summary.SprintHealth, _summary.SprintHourRateVariance);
            _summary.MonthHealthStatus = HealthInspector.GetMonthStatus(_summary.MonthHealth, _summary.MonthHourRateVariance);
        }

        private void SetErrors()
        {
            SetAuthorsWithErrors();
            SetUnassignedErrors();

            if (!_summary.AuthorsWithErrors.IsEmpty() || !_summary.UnassignedErrors.IsEmpty())
                _summary.HasErrors = true;
        }

        private void SetAuthorsWithErrors()
        {
            _summary.AuthorsWithErrors = new List<JiraAuthor>();
            _summary.AuthorsWithErrors = _summary.Authors.FindAll(a => !a.Errors.IsEmpty()).ToList();
        }

        private void SetUnassignedErrors()
        {
            if (!_summary.HasSprint || _report.ReportTasks.UnassignedTasksVisible.IsEmpty())
                return;

            _summary.UnassignedErrors = new List<Error>();

            var noRemainingErrors = new List<Error>();
            var completedTasksErrors = new List<Error>();

            if (!_report.ReportTasks.UnassignedTasksVisible.IsEmpty())
                noRemainingErrors = _report.ReportTasks.UnassignedTasksVisible.Where(t => t.ErrorsCount > 0).SelectMany(e => e.Errors).ToList();
            if (!_report.ReportTasks.CompletedTasksVisible.IsEmpty())
                completedTasksErrors = _report.ReportTasks.CompletedTasksVisible.Where(t => t.ErrorsCount > 0 && t.Assignee == null).SelectMany(e => e.Errors).ToList();

            _summary.UnassignedErrors = noRemainingErrors.Concat(completedTasksErrors).ToList();

            var errorContext = new ErrorContext(_summary.UnassignedErrors, null);
            _summary.UnassignedErrorsMessageHeader = ErrorService.GetMessagesHeader(errorContext);
            _summary.UnassignedErrorsMessageList = ErrorService.GetMessagesList(errorContext);
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

        private void SetCharts()
        {
            var widthHelper = new SummaryWidthLoader(_summary.SummaryChartWidth, _summary.StatusChartWidth);
            widthHelper.SetWorkSummaryChartWidths(_summary, _summary.WorkSummaryMaxValue, _report.IsIndividualDraft);

            if (_report.IsIndividualDraft)
                return;

            GetStatusValues();
            widthHelper.SetStatusElementsWidth(_summary);
        }

        private void GetStatusValues()
        {
            GetStatusSprintValues();
            GetStatusMonthValues();
            GetReportDayValues();
        }

        private void GetReportDayValues()
        {
            _summary.ReportDay = new ChartElement();
            _summary.ReportDay.ActualValueSeconds = _summary.Timing.AverageWorked;
            _summary.ReportDay.ActualValue = _summary.Timing.AverageWorkedString;
        }

        private void GetStatusSprintValues()
        {
            _summary.SprintEstimated = new ChartElement();
            _summary.SprintEstimated.ActualValueSeconds = _summary.Timing.SprintAverageEstimate;
            _summary.SprintEstimated.ActualValue = _summary.Timing.SprintAverageEstimateString;

            _summary.SprintDone = new ChartElement();
            _summary.SprintDone.ActualValueSeconds = _summary.Timing.AverageWorkedSprint;
            _summary.SprintDone.ActualValue = _summary.Timing.AverageWorkedSprintString;

            _summary.SprintRemaining = new ChartElement();
            _summary.SprintRemaining.ActualValueSeconds = _summary.Timing.RemainingSprintAverage;
            _summary.SprintRemaining.ActualValue = _summary.Timing.RemainingSprintAverageString;
        }

        private void GetStatusMonthValues()
        {
            _summary.MonthEstimated = new ChartElement();
            _summary.MonthEstimated.ActualValueSeconds = _summary.Timing.MonthAverageEstimated;
            _summary.MonthEstimated.ActualValue = _summary.Timing.MonthAverageEstimatedString;

            _summary.MonthDone = new ChartElement();
            _summary.MonthDone.ActualValueSeconds = _summary.Timing.AverageWorkedMonth;
            _summary.MonthDone.ActualValue = _summary.Timing.AverageWorkedMonthString;

            _summary.MonthRemaining = new ChartElement();
            _summary.MonthRemaining.ActualValueSeconds = _summary.Timing.RemainingMonthAverage;
            _summary.MonthRemaining.ActualValue = _summary.Timing.RemainingMonthAverageString;
        }

        private WorkingDaysInfo LoadWorkingDaysInfo()
        {
            var workingDaysInfo = new WorkingDaysInfo();
            var now = DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc);
            var reportDate = _report.ToDate.AddDays(-1);
            var reportMonthStart = _report.FromDate.StartOfMonth();
            var reportMonthEnd = _report.FromDate.EndOfMonth();

            workingDaysInfo.ReportWorkingDays = SummaryHelpers.GetWorkingDays(_options.FromDate, reportDate.AddDays(1), _report.WorkingDaysContext);

            workingDaysInfo.MonthWorkingDays = SummaryHelpers.GetWorkingDays(reportMonthStart, reportMonthEnd.AddDays(1), _report.WorkingDaysContext);

            workingDaysInfo.MonthWorkingDaysLeft = SummaryHelpers.GetWorkingDays(reportDate.AddDays(1), reportMonthEnd.AddDays(1), _report.WorkingDaysContext);

            if(reportMonthEnd <= reportDate)
            {
                workingDaysInfo.MonthWorkedDays = SummaryHelpers.GetWorkingDays(reportMonthStart, reportMonthEnd.AddDays(1), _report.WorkingDaysContext);
            }
            else
            {
                workingDaysInfo.MonthWorkedDays = SummaryHelpers.GetWorkingDays(reportMonthStart, reportDate.AddDays(1), _report.WorkingDaysContext);
            }


            if (_sprint != null)
            {
                var sprintEndDate = _sprint.EndDateDateTime.ToOriginalTimeZone(_report.OffsetFromUtc);
                var sprintStartDate = _sprint.StartDateDateTime.ToOriginalTimeZone(_report.OffsetFromUtc);
                workingDaysInfo.SprintWorkingDaysLeft = SummaryHelpers.GetWorkingDays(DateTime.Now.ToOriginalTimeZone(_report.OffsetFromUtc), sprintEndDate.Value.AddDays(1), _report.WorkingDaysContext);
                workingDaysInfo.SprintWorkingDays = SummaryHelpers.GetWorkingDays(sprintStartDate.Value, sprintEndDate.Value.AddDays(1), _report.WorkingDaysContext);
                workingDaysInfo.SprintWorkedDays = SummaryHelpers.GetWorkingDays(sprintStartDate.Value, _report.ToDate, _report.WorkingDaysContext);
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

            _summary.StatusChartWidth = (int)(_summary.GuidelineInfoStatus.GuidelinesCount * _summary.GuidelineInfoStatus.GuidelineWidth);
            _summary.StatusMaxValue = _summary.GuidelineInfoStatus.GuidelinesRate * _summary.GuidelineInfoStatus.GuidelinesCount;
        }

        private void SetWorkSummaryGuidelines()
        {
            _summary.GuidelineInfoWorkSummary = new SummaryGuidelineInfo();
            _summary.GuidelineInfoWorkSummary.GuidelinesRate = GetGuidelinesRate(_summary.WorkSummaryMaxValue);
            _summary.GuidelineInfoWorkSummary.GuidelinesCount = GetGuidelinesCount(_summary.WorkSummaryMaxValue, _summary.GuidelineInfoWorkSummary.GuidelinesRate);
            _summary.GuidelineInfoWorkSummary.GuidelineWidth = GetGuidelineWidth(_summary.WorkSummaryMaxValue, _summary.GuidelineInfoWorkSummary.GuidelinesRate);

            _summary.SummaryChartWidth = (int)(_summary.GuidelineInfoWorkSummary.GuidelinesCount * _summary.GuidelineInfoWorkSummary.GuidelineWidth);
            _summary.WorkSummaryMaxValue = _summary.GuidelineInfoWorkSummary.GuidelinesRate * _summary.GuidelineInfoWorkSummary.GuidelinesCount;
        }

        private void SetGuidelinesForAuthors()
        {
            foreach (var author in _summary.Authors)
            {
                author.GuidelineInfo = new SummaryGuidelineInfo();
                author.GuidelineInfo.GuidelinesRate = GetGuidelinesRate(author.MaxHourValue);
                author.GuidelineInfo.GuidelinesCount = GetGuidelinesCount(author.MaxHourValue, author.GuidelineInfo.GuidelinesRate);
                author.GuidelineInfo.GuidelineWidth = GetGuidelineWidth(author.MaxHourValue, author.GuidelineInfo.GuidelinesRate);
                author.MaxBarWidth = (int)(author.GuidelineInfo.GuidelinesCount * author.GuidelineInfo.GuidelineWidth);
                author.MaxHourValue = author.GuidelineInfo.GuidelinesCount * author.GuidelineInfo.GuidelinesRate;
            }
        }

        private int GetGuidelinesRate(int maxValue)
        {
            var integer = 2;
            while (maxValue / integer > _summary.GuidelinesOptimalNumber)
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
            var width = MathHelpers.RuleOfThree(200, maxValue, guidelinesRate);
            return MathHelpers.RoundToNextEvenInteger(width);
        }

    }
}
