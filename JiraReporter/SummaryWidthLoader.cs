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
        public int ChartMaxBarWidth { get; set; }

        public SummaryWidthLoader(int chartMaxBarWidth)
        {
            ChartMaxBarWidth = chartMaxBarWidth;
        }

        public void SetWorkSummaryChartWidths(Summary summary, int workSummaryMax, bool isIndividualDraft)
        {
            SetAuthorsWorkSummaryChartWidths(summary.Authors, workSummaryMax, isIndividualDraft);

            if (!isIndividualDraft)
                SetRemainingUnassignedWidths(summary, workSummaryMax);
        }

        private void SetRemainingUnassignedWidths(Summary summary, int workSummaryMax)
        {
            summary.UnassignedRemaining = new ChartElement();
            summary.UnassignedRemaining.ActualValueSeconds = summary.Timing.UnassignedTasksHoursAverageLeft;
            summary.UnassignedRemaining.ActualValue = summary.Timing.UnassignedTasksTimeLeftString;
            SetChartElementWidth(workSummaryMax, summary.UnassignedRemaining);
        }

        private void SetAuthorsWorkSummaryChartWidths(List<JiraAuthor> authors, int workSummaryMax, bool isIndividualDraft)
        {
            foreach (var author in authors)
                if (isIndividualDraft)
                    AuthorHelpers.SetAuthorWorkSummaryWidths(author, ChartMaxBarWidth, author.MaxHourValue);
                else
                    AuthorHelpers.SetAuthorWorkSummaryWidths(author, ChartMaxBarWidth, workSummaryMax);
        }

        public void SetStatusElementsWidth(Summary summary)
        {
            SetChartElementWidth(summary.StatusMaxValue, summary.SprintDay);
            SetChartElementWidth(summary.StatusMaxValue, summary.SprintDone);
            SetChartElementWidth(summary.StatusMaxValue, summary.SprintEstimated);
            SetChartElementWidth(summary.StatusMaxValue, summary.SprintRemaining);

            SetChartElementWidth(summary.StatusMaxValue, summary.MonthDone);
            SetChartElementWidth(summary.StatusMaxValue, summary.MonthEstimated);
            SetChartElementWidth(summary.StatusMaxValue, summary.MonthRemaining);
        }

        private void SetChartElementWidth(int maxWidth, ChartElement chartElement)
        {
            chartElement.Width = MathHelpers.RuleOfThree(ChartMaxBarWidth, maxWidth, (chartElement.ActualValueSeconds / 3600));
        }
    }
}
