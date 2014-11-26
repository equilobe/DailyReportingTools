using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class HealthInspector
    {
        private static string GetHealthStatus(int deviation, Health health, string specificString)
        {
            var statusProcessor = new Dictionary<Health, string>()
            {
                {Health.Bad, GetBadHealthStatus(deviation, specificString)},
                {Health.Weak, GetWeakHealthStatus(deviation)},
                {Health.Good, GetGoodHealthStatus()},
                {Health.None, GetEmptyHealthStatus()}
            };
            return statusProcessor[health];
        }

        private static string GetBadHealthStatus(int deviation, string specificString)
        {
            if (deviation > 0)
                return "Remove sprint tasks or increase " + specificString;
            else
                return "Add tasks to sprint";
        }

        private static string GetWeakHealthStatus(int deviation)
        {
            if (deviation > 0)
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

        public static Health GetWorkedDaysHealth(int allocatedHours, int totalTimeHours)
        {
           // int workedDays = GetWorkingDays(FromDate, ToDate);
           // int allocatedHours = AllocatedHoursPerDay * workedDays;
            if (allocatedHours > 0)
                return GetHealthFromPercentage(allocatedHours, totalTimeHours);
            else
                return Health.None;
        }

        public static Health GetDayHealth(Dictionary<TimesheetType, Timesheet> timesheetCollection, int allocatedHours, int sprintHourRate)
        {
            if (allocatedHours > 0 && timesheetCollection.TimesheetExists(TimesheetType.SprintTimesheet))
                return GetHealthFromPercentage(allocatedHours, sprintHourRate);
            else
                return Health.None;
        }

        public static Health GetHealthFromPercentage(int allocatedTime, int hourRate)
        {
            int diff = Summary.GetDeviation(allocatedTime, hourRate);
            if (diff < 0)
                diff = diff * (-1);

            if (GetPercentage(diff, allocatedTime) <= 5)
                return Health.Good;
            else if (GetPercentage(diff, allocatedTime) <= 15)
                return Health.Weak;
            else
                return Health.Bad;        
        }

        public static int GetPercentage(int value, int total)
        {
            var percentage = value * 100;
            return percentage / total;
        }
    }
 }
