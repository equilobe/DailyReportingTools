using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JiraReporter.Model;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Utils;
using JiraReporter.Helpers;

namespace JiraReporter.Helpers
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

            TimingHelpers.SetAverageWorkStringFormat(author.Timing);
        }

        public static void SetAuthorAverageRemainig(JiraAuthor author, int sprintWorkingDaysLeft)
        {
            if (sprintWorkingDaysLeft > 0)
                author.Timing.TotalRemainingAverage = author.Timing.TotalRemainingHours / sprintWorkingDaysLeft;
            author.Timing.TotalRemainingString = author.Timing.TotalRemainingAverage.RoundDoubleOneDecimal();
        }

        public static double GetAuthorMaxAverage(JiraAuthor author)
        {
            var max = Math.Max(author.Timing.AverageWorked, author.Timing.AverageWorkedSprint);
            max = Math.Max(max, author.Timing.AverageWorkedMonth);
            return Math.Max(max, author.Timing.TotalRemainingAverage * 3600);
        }

        public static void SetAuthorWorkSummaryWidths(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.SprintChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedSprint / 3600));
            author.MonthChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedMonth / 3600));
            author.DayChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.Timing.AverageWorked / 3600));
            author.RemainingChartPixelWidth = MathHelpers.RuleOfThree(maxWidth, maxValue, author.Timing.TotalRemainingAverage);
        }
    }
}
