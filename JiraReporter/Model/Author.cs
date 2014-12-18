using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Author
    {
        public string Name { get; set; }
        public string ShortName
        {
            get
            {
                return AuthorsProcessing.GetShortName(Name);
            }
        }
        public string FirstName
        {
            get
            {
                return AuthorsProcessing.GetFirstName(Name);
            }
        }
        public string Initials { get; set; }

        public string TimeLogged { get; set; }
        public int TimeSpent { get; set; }
        public double TimeSpentHours
        {
            get
            {
                return TimeSpent / 3600;
            }
        }
        public string TimeSpentHoursString
        {
            get
            {
                return TimeSpent.SetTimeFormat();
            }
        }

        public int TimeLoggedPerDayAverage { get; set; }
        public string TimeLoggedPerDayAverageString
        {
            get
            {
                return TimeLoggedPerDayAverage.SetTimeFormat8Hour();
            }
        }

        public int TimeSpentCurrentMonthSeconds { get; set; }
        public double TimeSpentCurrentMonthHours
        {
            get
            {
                return (double)TimeSpentCurrentMonthSeconds / 3600;
            }
        }
        public string TimeSpentCurrentMonthHoursString
        {
            get
            {
                return TimeSpentCurrentMonthSeconds.SetTimeFormat8Hour();
            }
        }
        public double MonthWorkedPerDay { get; set; }
        public string MonthWorkedPerDayString
        {
            get
            {
                return ((int)(MonthWorkedPerDay)).SetTimeFormat8Hour();
            }
        }
        public int TimeSpentCurrentSprintSeconds { get; set; }
        public double TimeSpentCurrentSprintHours
        {
            get
            {
                return (double)TimeSpentCurrentSprintSeconds / 3600;
            }
        }
        public string TimeSpentCurrentSprintString
        {
            get
            {
                return TimeSpentCurrentSprintSeconds.SetTimeFormat8Hour();
            }
        }
        public double SprintWorkedPerDay { get; set; }
        public string SprintWorkedPerDayString
        {
            get
            {
                return ((int)(SprintWorkedPerDay)).SetTimeFormat8Hour();
            }
        }

        public double DayChartPixelWidth { get; set; }
        public string DayChartPixelWidthString
        {
            get
            {
                return DayChartPixelWidth.ToString() + "px";
            }
        }
        public double SprintChartPixelWidth { get; set; }
        public string SprintChartPixelWidthString
        {
            get
            {
                return SprintChartPixelWidth.ToString() + "px";
            }
        }
        public double MonthChartPixelWidth { get; set; }
        public string MonthChartPixelWidthString
        {
            get
            {
                return MonthChartPixelWidth.ToString() + "px";
            }
        }

        public List<Issue> Issues { get; set; }
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public List<Issue> InProgressTasksParents { get; set; }
        public List<Issue> OpenTasksParents { get; set; }

        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int RemainingEstimateSeconds { get; set; }
        public double RemainingEstimateHours { get; set; }
        public string RemainingEstimateHoursString
        {
            get
            {
                return RemainingEstimateSeconds.SetTimeFormat8Hour();
            }
        }
        public List<Commit> Commits { get; set; }
        public List<DayLog> DayLogs { get; set; }
        public List<Error> Errors { get; set; }
    }
}
