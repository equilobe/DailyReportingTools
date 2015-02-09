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
        public static int GetTimesheetSecondsWorked(this Timesheet timesheet)
        {
            return timesheet.Worklog.Issues.Where(issue=>issue!=null).Sum(i => i.Entries.Sum(t => t.TimeSpent));
        }

        public static int GetTimesheetSecondsWorkedAuthor(this Timesheet timesheet, JiraAuthor author)
        {
            int seconds = 0;
            foreach (var issue in timesheet.Worklog.Issues)
                seconds += issue.Entries.Where(e => e.AuthorFullName == author.Name).Sum(e => e.TimeSpent);
            return seconds;
        }
    }
}
