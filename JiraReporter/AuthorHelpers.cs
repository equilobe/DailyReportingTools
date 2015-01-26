using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JiraReporter.Model;

namespace JiraReporter
{
    static class AuthorHelpers
    {
        public static string GetCleanName(string name)
        {
            string delimiter = "(\\[.*\\])";
            if (name != null)
                name = Regex.Replace(name, delimiter, "");
            return name;
        }

        public static string GetFirstName(string name)
        {
            var names = name.Split(' ');
            return names[0];
        }

        public static string GetShortName(string name)
        {
            var names = name.Split(' ');
            if (names.Count() > 1)
                return names[0] + " " + names[1][0] + ".";
            else
                return names[0];
        }


        public static List<Commit> GetDayLogCommits(Author author, DateTime date)
        {
            var commits = new List<Commit>();
            if (author.Commits != null)
                commits = author.Commits.FindAll(c => c.Entry.Date.ToOriginalTimeZone() >= date && c.Entry.Date.ToOriginalTimeZone() < date.AddDays(1));
            return commits;
        }


        public static List<string> GetAuthorsFromTimesheet(Timesheet timesheet)
        {
            var authors = new List<string>();
            if (timesheet != null && timesheet.Worklog != null)
                authors = timesheet.Worklog.Issues.SelectMany(i => i.Entries.Select(e => e.AuthorFullName)).Distinct().ToList();
            return authors;
        }

        public static void SetAuthorAverageWorkPerDay(Author author, int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            if (monthWorkedDays == 0)
                author.MonthWorkedPerDay = 0;
            else
                author.MonthWorkedPerDay = author.TimeSpentCurrentMonthSeconds / monthWorkedDays;
            if (sprintWorkedDays == 0)
                author.SprintWorkedPerDay = 0;
            else
                author.SprintWorkedPerDay = author.TimeSpentCurrentSprintSeconds / sprintWorkedDays;
            if (reportWorkingDays == 0)
                author.TimeLoggedPerDayAverage = 0;
            else
                author.TimeLoggedPerDayAverage = author.TimeSpent / reportWorkingDays;
        }

        public static double GetAuthorMaxAverage(Author author)
        {
            var max = Math.Max(author.TimeLoggedPerDayAverage, author.SprintWorkedPerDay);
            max = Math.Max(max, author.MonthWorkedPerDay);
            return max;
        }

        public static void SetAuthorWorkSummaryWidths(Author author, int maxWidth, int maxValue)
        {
            author.SprintChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.SprintWorkedPerDay / 3600));
            author.MonthChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.MonthWorkedPerDay / 3600));
            author.DayChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.TimeLoggedPerDayAverage / 3600));
        }
    }
}
