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
        public double TimeSpent { get; set; }
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
                return TimeFormatting.SetTimeFormat8Hour((int)(TimeSpentHours * 3600));
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
                return TimeFormatting.SetTimeFormat8Hour(TimeSpentCurrentMonthSeconds);
            }
        }
        public double MonthWorkedPerDay { get; set; }
        public string MonthWorkedPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(MonthWorkedPerDay));
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
                return TimeFormatting.SetTimeFormat8Hour(TimeSpentCurrentSprintSeconds);
            }
        }
        public double SprintWorkedPerDay { get; set; }
        public string SprintWorkedPerDayString
        {
            get
            {
                return TimeFormatting.SetTimeFormat8Hour((int)(SprintWorkedPerDay));
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
               return TimeFormatting.SetTimeFormat8Hour(RemainingEstimateSeconds);
            }
        }
        public List<Commit> Commits { get; set; }
        public List<DayLog> DayLogs { get; set; }
        public List<Error> Errors { get; set; }
    }
}
