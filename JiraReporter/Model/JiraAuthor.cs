using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class JiraAuthor
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string ShortName
        {
            get
            {
                return AuthorHelpers.GetShortName(Name);
            }
        }
        public string FirstName
        {
            get
            {
                return AuthorHelpers.GetFirstName(Name);
            }
        }
        public string Initials { get; set; }
        public string EmailAdress { get; set; }
        public Timing Timing { get; set; }

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
        public Timesheet CurrentTimesheet { get; set; }
        public Timesheet MonthTimesheet { get; set; }
        public Timesheet SprintTimesheet { get; set; }

        public List<Issue> Issues { get; set; }
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public List<Issue> InProgressTasksParents { get; set; }
        public List<Issue> OpenTasksParents { get; set; }

        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }

        public int RemainingEstimateSeconds { get; set; }
        public double RemainingEstimateHours { get; set; }
        public string RemainingEstimateHoursString
        {
            get
            {
                return RemainingEstimateSeconds.SetTimeFormat8Hour();
            }
        }
        public List<JiraCommit> Commits { get; set; }
        public List<JiraDayLog> DayLogs { get; set; }
        public List<Error> Errors { get; set; }
        public Uri AvatarLink { get; set; }
        public Image Image { get; set; }
        public IndividualDraftInfo IndividualDraftInfo { get; set; }
        public bool IsProjectLead { get; set; }

        public JiraAuthor()
        {

        }

        public JiraAuthor(JiraUser user)
        {
            Name = user.displayName;
            EmailAdress = user.emailAddress;
            Username = user.key;
            AvatarLink = user.avatarUrls.Big;
        }

        public bool HasIssues()
        {
            if (Issues == null || Issues.Count == 0)
                return false;

            return true;
        }
    }
}
