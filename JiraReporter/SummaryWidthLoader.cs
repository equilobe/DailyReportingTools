using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Helpers;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class SummaryWidthLoader
    {
        public int SummaryChartWidth { get; set; }
        public int StatusChartWidth { get; set; }

        public SummaryWidthLoader(int summaryChartWidth, int statusChartWidth)
        {
            SummaryChartWidth = summaryChartWidth;
            StatusChartWidth = statusChartWidth;
        }

        public void SetWorkSummaryChartWidths(Summary summary, int workSummaryMax, bool isIndividualDraft)
        {
            SetAuthorCharts(summary.Authors, workSummaryMax, isIndividualDraft);

            if (!isIndividualDraft)
                SetRemainingUnassignedWidths(summary, workSummaryMax);
        }

        private void SetRemainingUnassignedWidths(Summary summary, int workSummaryMax)
        {
            summary.UnassignedRemaining = new ChartElement();
            summary.UnassignedRemaining.ActualValueSeconds = summary.Timing.UnassignedTasksHoursAverageLeft * 3600;
            summary.UnassignedRemaining.ActualValue = summary.Timing.UnassignedTasksTimeLeftString;
            SetChartElementWidth(summary.SummaryChartWidth, workSummaryMax, summary.UnassignedRemaining);
        }

        private void SetAuthorCharts(List<JiraAuthor> authors, int workSummaryMax, bool isIndividualDraft)
        {
            foreach (var author in authors)
                if (isIndividualDraft)
                    AuthorHelpers.SetAuthorCharts(author, author.MaxBarWidth, author.MaxHourValue);
                else
                    AuthorHelpers.SetAuthorCharts(author, SummaryChartWidth, workSummaryMax);
        }

        public void SetStatusElementsWidth(Summary summary)
        {
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.ReportDay);
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.SprintDone);
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.SprintEstimated);
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.SprintRemaining);

            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.MonthDone);
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.MonthEstimated);
            SetChartElementWidth(summary.StatusChartWidth, summary.StatusMaxValue, summary.MonthRemaining);
        }

        private void SetChartElementWidth(int maxWidth, int maxValue, ChartElement chartElement)
        {
            chartElement.Width = MathHelpers.RuleOfThree(maxWidth, maxValue, (chartElement.ActualValueSeconds / 3600));
        }
    }
}
