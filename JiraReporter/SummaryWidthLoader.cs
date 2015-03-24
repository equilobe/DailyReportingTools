﻿using Equilobe.DailyReport.Models.ReportFrame;
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

        public void SetWorkSummaryChartWidths(Summary summary, int workSummaryMax)
        {
            SetAuthorsWorkSummaryChartWidths(summary.Authors, workSummaryMax);
            SetRemainingUnassignedWidths(summary, workSummaryMax);
        }

        private void SetRemainingUnassignedWidths(Summary summary, int workSummaryMax)
        {
            summary.UnassignedRemainingChartPixelWidth = MathHelpers.RuleOfThree(ChartMaxBarWidth, workSummaryMax, summary.Timing.UnassignedTasksHoursAverageLeft);
        }

        private void SetAuthorsWorkSummaryChartWidths(List<JiraAuthor> authors, int workSummaryMax)
        {
            foreach (var author in authors)
                AuthorHelpers.SetAuthorWorkSummaryWidths(author, ChartMaxBarWidth, workSummaryMax);
        }

        public void SetStatusChartWidths(int maxStatusWidth, StatusChartWidths sprint, StatusChartWidths month)
        {
            SetStatusElementWidths(maxStatusWidth, sprint);
            SetStatusElementWidths(maxStatusWidth, month);
        }

        private void SetStatusElementWidths(int maxStatusWidth, StatusChartWidths statusElement)
        {
            statusElement.DayWidth = MathHelpers.RuleOfThree(ChartMaxBarWidth, maxStatusWidth, (statusElement.DaySeconds / 3600));
            statusElement.DoneWidth = MathHelpers.RuleOfThree(ChartMaxBarWidth, maxStatusWidth, (statusElement.DoneSeconds / 3600));
            statusElement.EstimatedWidth = MathHelpers.RuleOfThree(ChartMaxBarWidth, maxStatusWidth, (statusElement.EstimatedSeconds / 3600));
            statusElement.RemainingWidth = MathHelpers.RuleOfThree(ChartMaxBarWidth, maxStatusWidth, (statusElement.RemainingSeconds / 3600));
        }
    }
}