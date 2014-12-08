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
        public List<int> NonWorkingDays { get; set; }
        public HealthInspector(List<int> nonWorkingDays)
        {
            NonWorkingDays = nonWorkingDays;
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
           // int workedDays = GetWorkingDays(FromDate, ToDate);
           // int allocatedHours = AllocatedHoursPerDay * workedDays;
            if (allocatedHours > 0)
                return GetHealthFromPercentage(allocatedHours, totalTimeHours);
            else
                return Health.None;
        }

        public Health GetDayHealth(double allocatedHours, double sprintHourRate)
        {
            if (allocatedHours > 0)
                return GetHealthFromPercentage(allocatedHours, sprintHourRate);
            else
                return Health.None;
        }

        public Health GetSprintHealth(Timesheet sprint, double allocatedHours, double totalTime)
        {
            int daysWorked = 0;
            if (allocatedHours > 0)
            {
                if (DateTime.Now.ToOriginalTimeZone() <= sprint.EndDate.AddDays(-1).ToOriginalTimeZone())
                    daysWorked = Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), DateTime.Now.ToOriginalTimeZone().AddDays(-1).Date, NonWorkingDays);
                else
                    daysWorked = Summary.GetWorkingDays(sprint.StartDate.ToOriginalTimeZone(), sprint.EndDate.ToOriginalTimeZone().AddDays(-1), NonWorkingDays);
                return GetHealthFromPercentage(allocatedHours * daysWorked, totalTime);
            }
            else
                return Health.None;
        }

        public Health GetMonthHealth(double allocatedHours, double totalTimeWorked)
        {
            if (allocatedHours > 0)
            {
                var workedDays = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1), NonWorkingDays);
                var workedPerDay = totalTimeWorked / workedDays;
                var monthWorkingDays = Summary.GetWorkingDays(DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().EndOfMonth(), NonWorkingDays);
                var averageFromAllocatedHours = allocatedHours / monthWorkingDays;
                return GetHealthFromPercentage(averageFromAllocatedHours, workedPerDay);
            }
            else
                return Health.None;
        }

        public Health GetHealthFromPercentage(double allocatedTime, double hourRate)
        {
            var variance = Summary.GetVariance(allocatedTime, hourRate);
            if (variance < 0)
                variance = variance * (-1);

            if (GetPercentage(variance, allocatedTime) <= 5)
                return Health.Good;
            else if (GetPercentage(variance, allocatedTime) <= 15)
                return Health.Weak;
            else
                return Health.Bad;        
        }

        public static double GetPercentage(double value, double total)
        {
            var percentage = (value * 100)/total;
            return percentage;
        }
    }
 }
