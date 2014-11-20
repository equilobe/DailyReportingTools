using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public static class TimesheetExtensions
    {
        public static double GetTimesheetSecondsWorked(this Timesheet timesheet)
        {
            return (double)timesheet.Worklog.Issues.Sum(i => i.Entries.Sum(t => t.TimeSpent));
        }

        public static double GetTimesheetSecondsWorkedAuthor(this Timesheet timesheet, Author author)
        {
            double seconds = 0;
            foreach (var issue in timesheet.Worklog.Issues)
                seconds += issue.Entries.Where(e => e.AuthorFullName == author.Name).Sum(e => e.TimeSpent);
            return seconds;
        }
    }
}
