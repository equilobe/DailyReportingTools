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


        public static List<JiraCommit> GetDayLogCommits(JiraAuthor author, DateTime date)
        {
            var commits = new List<JiraCommit>();
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

        public static void SetAuthorAverageWorkPerDay(JiraAuthor author, int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays)
        {
            if (monthWorkedDays == 0)
                author.AverageWorkedMonth = 0;
            else
                author.AverageWorkedMonth = author.MonthSecondsWorked / monthWorkedDays;
            if (sprintWorkedDays == 0)
                author.SprintWorkedPerDay = 0;
            else
                author.SprintWorkedPerDay = author.SprintSecondsWorked / sprintWorkedDays;
            if (reportWorkingDays == 0)
                author.AverageWorked = 0;
            else
                author.AverageWorked = author.TimeSpent / reportWorkingDays;
        }

        public static double GetAuthorMaxAverage(JiraAuthor author)
        {
            var max = Math.Max(author.AverageWorked, author.SprintWorkedPerDay);
            max = Math.Max(max, author.AverageWorkedMonth);
            return max;
        }

        public static void SetAuthorWorkSummaryWidths(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.SprintChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.SprintWorkedPerDay / 3600));
            author.MonthChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.AverageWorkedMonth / 3600));
            author.DayChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.AverageWorked / 3600));
        }
    }
}
