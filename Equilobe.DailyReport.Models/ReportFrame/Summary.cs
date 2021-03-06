﻿using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class Summary
    {
        public JiraPolicy Policy { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ReportDate { get; set; }
        public bool IsFinalDraft { get; set; }

        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int InProgressUnassignedCount { get; set; }
        public int OpenUnassignedCount { get; set; }
        public int UnassignedTasksCount { get; set; }

        public TimingDetailed Timing { get; set; }
        public List<JiraAuthor> Authors { get; set; }
        public int CommitsCount { get; set; }
        public List<JiraPullRequest> PullRequests { get; set; }
        public List<JiraPullRequest> UnrelatedPullRequests { get; set; }
        public Sprint Sprint { get; set; }

        public bool HasMonth { get; set; }
        public bool HasStatus { get; set; }
        public bool HasWorkSummary { get; set; }
        public bool HasSprint { get; set; }

        public bool HasErrors { get; set; }
        public List<Error> UnassignedErrors { get; set; }
        public List<JiraAuthor> AuthorsWithErrors { get; set; }
        public string UnassignedErrorsMessageHeader { get; set; }
        public List<string> UnassignedErrorsMessageList { get; set; }

        public WorkingDaysInfo WorkingDays { get; set; }

        public Health WorkedDaysHealth { get; set; }
        public Health DayHealth { get; set; }
        public Health SprintHealth { get; set; }
        public Health MonthHealth { get; set; }

        public Dictionary<Health, string> HealthColors { get; set; }
        public string SprintHealthStatus { get; set; }
        public string MonthHealthStatus { get; set; }

        public string SprintStatus { get; set; }
        public string MonthStatus { get; set; }
        public string MonthName { get; set; }

        public ChartElement ReportDay { get; set; }

        public ChartElement SprintEstimated { get; set; }
        public ChartElement SprintDone { get; set; }
        public ChartElement SprintRemaining { get; set; }

        public ChartElement MonthEstimated { get; set; }
        public ChartElement MonthDone { get; set; }
        public ChartElement MonthRemaining { get; set; }

        public ChartElement UnassignedRemaining { get; set; }

        public SummaryGuidelineInfo GuidelineInfoStatus { get; set; }
        public SummaryGuidelineInfo GuidelineInfoWorkSummary { get; set; }

        public int WorkSummaryMaxValue { get; set; }
        public int StatusMaxValue { get; set; }

        public double SprintHourRateVariance { get; set; }
        public double MonthHourRateVariance { get; set; }

        public readonly int ChartMaxWidth = 300;
        public int StatusChartWidth { get; set; }
        public int SummaryChartWidth { get; set; }


        public readonly int GuidelinesOptimalNumber = 12;
    }
}