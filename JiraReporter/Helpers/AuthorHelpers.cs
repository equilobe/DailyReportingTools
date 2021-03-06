﻿using System;
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
            var names = RemoveMultipleSpaces(name).Split(' ');
            if (names.Count() > 1)
                return names[0] + " " + names[1][0] + ".";
            else
                return names[0];
        }

        static string RemoveMultipleSpaces(string word)
        {
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            return regex.Replace(word, @" ");
        }


        public static List<JiraCommit> GetDayLogCommits(JiraAuthor author, DateTime date, TimeSpan offsetFromUtc)
        {
            var commits = new List<JiraCommit>();
            if (author.Commits != null)
                commits = author.Commits.FindAll(c => c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) >= date && c.Entry.Date.ToOriginalTimeZone(offsetFromUtc) < date.AddDays(1));
            return commits;
        }

        public static void SetAuthorAverageWorkPerDay(JiraAuthor author, int monthWorkedDays, int sprintWorkedDays, int reportWorkingDays, bool hasSprint)//TODO: refactor parameters
        {
            if (monthWorkedDays == 0)
                author.Timing.AverageWorkedMonth = 0;
            else
                author.Timing.AverageWorkedMonth = author.Timing.MonthSecondsWorked / monthWorkedDays;

            if (hasSprint)
            {
                if (sprintWorkedDays == 0)
                    author.Timing.AverageWorkedSprint = 0;
                else
                    author.Timing.AverageWorkedSprint = author.Timing.SprintSecondsWorked / sprintWorkedDays;
            }
            else
            {
                author.Timing.AverageWorkedLast7Days = author.Timing.Last7DaySecondsWorked / 7;
            }
    
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
            else
                author.Timing.TotalRemainingAverage = author.Timing.TotalRemainingHours;
            author.Timing.TotalRemainingString = author.Timing.TotalRemainingAverage.RoundDoubleOneDecimal();
        }

        public static double GetAuthorMaxAverage(JiraAuthor author)
        {
            var max = Math.Max(author.Timing.AverageWorked, author.Timing.AverageWorkedSprint);
            max = Math.Max(max, author.Timing.AverageWorkedMonth);
            return Math.Max(max, author.Timing.TotalRemainingAverage * 3600);
        }

        public static void SetAuthorCharts(JiraAuthor author, int maxWidth, int maxValue)
        {
            SetDayChart(author, maxWidth, maxValue);

            SetSprintChart(author, maxWidth, maxValue);

            SetMonthChart(author, maxWidth, maxValue);

            SetRemainingChart(author, maxWidth, maxValue);
        }

        private static void SetDayChart(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.Day = new ChartElement();
            author.Day.ActualValueSeconds = author.Timing.AverageWorked;
            author.Day.ActualValue = author.Timing.AverageWorkedString;
            author.Day.Width = MathHelpers.RuleOfThree(maxWidth, maxValue, ((double)author.Timing.AverageWorked / 3600));
        }

        private static void SetSprintChart(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.Sprint = new ChartElement();
            author.Sprint.ActualValueSeconds = author.Timing.AverageWorkedSprint;
            author.Sprint.ActualValue = author.Timing.AverageWorkedSprintString;
            author.Sprint.Width = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedSprint / 3600));
        }

        private static void SetMonthChart(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.Month = new ChartElement();
            author.Month.ActualValueSeconds = author.Timing.AverageWorkedMonth;
            author.Month.ActualValue = author.Timing.AverageWorkedMonthString;
            author.Month.Width = MathHelpers.RuleOfThree(maxWidth, maxValue, (author.Timing.AverageWorkedMonth / 3600));
        }

        private static void SetRemainingChart(JiraAuthor author, int maxWidth, int maxValue)
        {
            author.Remaining = new ChartElement();
            author.Remaining.ActualValueSeconds = author.Timing.TotalRemainingAverage * 3600;
            author.Remaining.ActualValue = author.Timing.TotalRemainingString;
            author.Remaining.Width = MathHelpers.RuleOfThree(maxWidth, maxValue, author.Timing.TotalRemainingAverage);
        }
    }
}
