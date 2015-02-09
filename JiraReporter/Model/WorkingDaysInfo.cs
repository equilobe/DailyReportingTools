using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class WorkingDaysInfo
    {
        public int ReportWorkingDays { get; set; }
        public int MonthWorkingDays { get; set; }
        public int MonthWorkingDaysLeft { get; set; }
        public int MonthWorkedDays { get; set; }
        public int SprintWorkingDays { get; set; }
        public int SprintWorkingDaysLeft { get; set; }
        public int SprintWorkedDays { get; set; }

        public WorkingDaysInfo(Sprint sprint, JiraPolicy policy, JiraOptions options)
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            ReportWorkingDays = SummaryHelpers.GetWorkingDays(options.FromDate, options.ToDate.AddDays(-1), policy.MonthlyOptions);
            MonthWorkingDays = SummaryHelpers.GetWorkingDays(now.StartOfMonth(), now.EndOfMonth(), policy.MonthlyOptions);
            MonthWorkingDaysLeft = SummaryHelpers.GetWorkingDays(now, now.EndOfMonth(), policy.MonthlyOptions);
            MonthWorkedDays = SummaryHelpers.GetWorkingDays(now.StartOfMonth(), now.AddDays(-1), policy.MonthlyOptions);
            if (sprint != null)
            {
                var sprintEndDate = sprint.EndDate.ToOriginalTimeZone();
                var sprintStartDate = sprint.StartDate.ToOriginalTimeZone();
                SprintWorkingDaysLeft = SummaryHelpers.GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), sprintEndDate, policy.MonthlyOptions);
                SprintWorkingDays = SummaryHelpers.GetWorkingDays(sprintStartDate, sprintEndDate, policy.MonthlyOptions);
                SprintWorkedDays = SummaryHelpers.GetSprintDaysWorked(sprint, policy);
            }
        }
    }
}
