using Equilobe.DailyReport.Models.Enums;
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

        public int MonthEstimatedPixelWidth { get; set; }
        public string MonthEstimatedPixelWidthString
        {
            get
            {
                return MonthEstimatedPixelWidth.ToString() + "px";
            }
        }

        public double UnassignedRemainingChartPixelWidth { get; set; }
        public string UnassignedRemainingChartWidthString
        {
            get
            {
                return UnassignedRemainingChartPixelWidth.ToString();
            }
        }
        public string UnassignedRemainingChartPixelWidthString
        {
            get
            {
                return UnassignedRemainingChartPixelWidth.ToString() + "px";
            }
        }

        public List<Error> Errors { get; set; }
        public List<Error> CompletedWithEstimateErrors { get; set; }
        public List<Error> CompletedWithNoWorkErrors { get; set; }
        public List<Error> UnassignedErrors { get; set; }
        public List<Error> ConfirmationErrors { get; set; }
        public List<JiraAuthor> AuthorsNotConfirmed { get; set; }
        public List<JiraAuthor> AuthorsWithErrors { get; set; }

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

        public SummaryGuidelineInfo GuidelineInfoStatus { get; set; }
        public SummaryGuidelineInfo GuidelineInfoWorkSummary { get; set; }

        public int WorkSummaryMaxValue { get; set; }
        public int StatusMaxValue { get; set; }

        public double SprintHourRateVariance { get; set; }
        public double MonthHourRateVariance { get; set; }

        public readonly int ChartMaxBarWidth = 250;
        public readonly int ChartMaxWidth = 300;
        public readonly int GuidelinesOptimalNumber = 12;

        public string ChartMaxWidthString
        {
            get
            {
                return ChartMaxWidth.ToString() + "px";
            }
        }
    }
}