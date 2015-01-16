using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class HealthInspector
    {
        SourceControlLogReporter.Model.Policy Policy { get; set; }

        public HealthInspector(SourceControlLogReporter.Model.Policy policy)
        {
            Policy = policy;
        }
        private static string GetHealthStatus(double variance, Health health, string specificString)
        {
            var statusProcessor = new Dictionary<Health, string>()
            {
                {Health.Bad, GetBadHealthStatus(variance, specificString)},
                {Health.Weak, GetWeakHealthStatus(variance)},
                {Health.Good, GetGoodHealthStatus()},
                {Health.None, GetEmptyHealthStatus()}
            };
            return statusProcessor[health];
        }

        private static string GetBadHealthStatus(double variance, string specificString)
        {
            if (variance > 0)
                return "Remove sprint tasks or increase " + specificString;
            else
                return "Add tasks to sprint";
        }

        private static string GetWeakHealthStatus(double variance)
        {
            if (variance > 0)
                return "Remove tasks from sprint";
            else
                return "Add tasks to sprint";
        }

        private static string GetGoodHealthStatus()
        {
            return "Good";
        }

        private static string GetEmptyHealthStatus()
        {
            return string.Empty;
        }

        public static string GetSprintStatus(Health sprintHealth, double variance)
        {
            string specificString = "sprint duration";
            return GetHealthStatus(variance, sprintHealth, specificString);
        }

        public static string GetMonthStatus(Health monthHealth, double variance)
        {
            string specificString = "monthly work limit";
            return GetHealthStatus(variance, monthHealth, specificString);
        }

        public Health GetWorkedDaysHealth(double allocatedHours, double totalTimeHours)
        {
            if (allocatedHours > 0)
                return GetHealthFromPercentage(allocatedHours, totalTimeHours);

            return Health.None;
        }

        public Health GetDayHealth(double allocatedHours, double sprintHourRate)
        {
            if (allocatedHours == 0)
                return Health.None;

            return GetHealthFromPercentage(allocatedHours, sprintHourRate);
        }

        public Health GetSprintHealth(Timesheet sprint, double allocatedHours, double totalTime)
        {
            if (allocatedHours == 0)
                return Health.None;

            int daysWorked = GetDaysWorked(sprint);
            return GetHealthFromPercentage(allocatedHours * daysWorked, totalTime);
        }

        private int GetDaysWorked(Timesheet sprint)
        {
            var now = DateTime.Now.ToOriginalTimeZone();
            if (now <= sprint.EndDate.AddDays(-1).ToOriginalTimeZone())
                return Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), now.AddDays(-1).Date, Policy.MonthlyOptions);

            return Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), sprint.EndDate.ToOriginalTimeZone().AddDays(-1), Policy.MonthlyOptions);
        }

        public Health GetMonthHealth(double allocatedHours, double totalTimeWorked)
        {
            if (allocatedHours == 0)
                return Health.None;

            var workedDays = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1), Policy.MonthlyOptions);
            var workedPerDay = totalTimeWorked / workedDays;
            var monthWorkingDays = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), Policy.MonthlyOptions);
            var averageFromAllocatedHours = allocatedHours / monthWorkingDays;
            return GetHealthFromPercentage(averageFromAllocatedHours, workedPerDay);
        }

        public Health GetHealthFromPercentage(double allocatedTime, double workedTime)
        {
            var variance = MathHelpers.GetVariance(allocatedTime, workedTime);
            if (variance < 0)
                variance = variance * (-1);
            var percentage = MathHelpers.GetPercentage(variance, allocatedTime);
            if (percentage <= 5)
                return Health.Good;
            else if (percentage <= 15)
                return Health.Weak;
            else
                return Health.Bad;
        }
    }
}
