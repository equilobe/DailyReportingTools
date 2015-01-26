using SourceControlLogReporter;
using SourceControlLogReporter.Model;
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

        public WorkingDaysInfo(Sprint sprint, Policy policy, Options options)
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            ReportWorkingDays = Summary.GetWorkingDays(options.FromDate, options.ToDate.AddDays(-1), policy.MonthlyOptions);
            MonthWorkingDays = Summary.GetWorkingDays(now.StartOfMonth(), now.EndOfMonth(), policy.MonthlyOptions);
            MonthWorkingDaysLeft = Summary.GetWorkingDays(now, now.EndOfMonth(), policy.MonthlyOptions);
            MonthWorkedDays = Summary.GetWorkingDays(now.StartOfMonth(), now.AddDays(-1), policy.MonthlyOptions);
            if (sprint != null)
            {
                var sprintEndDate = sprint.EndDate.ToOriginalTimeZone();
                var sprintStartDate = sprint.StartDate.ToOriginalTimeZone();
                SprintWorkingDaysLeft = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone(), sprintEndDate, policy.MonthlyOptions);
                SprintWorkingDays = Summary.GetWorkingDays(sprintStartDate, sprintEndDate, policy.MonthlyOptions);
                SprintWorkedDays = Summary.GetSprintDaysWorked(sprint, policy);
            }
        }
    }
}
