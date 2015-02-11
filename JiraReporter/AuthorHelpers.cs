using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JiraReporter.Model;
using Equilobe.DailyReport.Models.Jira;

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


        public static List<JiraCommit> GetDayLogCommits(JiraAuthor author, DateTime date, TimeSpan offsetFromUtc)
        {
            var commits = new List<JiraCommit>();
            if (author.Commits != null)
                commits = author.Commits.FindAll(c => c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) >= date && c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) < date.AddDays(1));
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
                author.Timing.AverageWorkedMonth = 0;
            else
                author.Timing.AverageWorkedMonth = author.Timing.MonthSecondsWorked / monthWorkedDays;
            if (sprintWorkedDays == 0)
                author.Timing.AverageWorkedSprint = 0;
            else
                author.Timing.AverageWorkedSprint = author.Timing.SprintSecondsWorked / sprintWorkedDays;
            if (reportWorkingDays == 0)
                author.Timing.AverageWorked = 0;
            else
                author.Timing.AverageWorked = author.Timing.TotalTimeSeconds / reportWorkingDays;

            TimeFormatting.SetAverageWorkStringFormat(author.Timing);
        }

        public static double GetAuthorMaxAverage(JiraAuthor author)
        {
            var max = Math.Max(author.Timing.AverageWorked, author.Timing.AverageWorkedSprint);
            max = Math.Max(max, author.Timing.AverageWorkedMonth);
            return max;
        }

        public static void SetAuthorWorkSummaryWidths(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.SprintChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedSprint / 3600));
            author.MonthChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedMonth / 3600));
            author.DayChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.Timing.AverageWorked / 3600));
        }
    }
}
