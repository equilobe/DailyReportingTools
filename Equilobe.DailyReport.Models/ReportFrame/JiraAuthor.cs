using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Policy;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraAuthor
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string UserKey { get; set; }
        public string ShortName { get; set; }
        public string FirstName { get; set; }
        public string Initials { get; set; }
        public string EmailAdress { get; set; }
        public Timing Timing { get; set; }

        public ChartElement Day { get; set; }
        public ChartElement Sprint { get; set; }
        public ChartElement Month { get; set; }
        public ChartElement Remaining { get; set; }

        public int MaxBarWidth { get; set; }

        public List<IssueDetailed> Issues { get; set; }
        public List<IssueDetailed> MonthIssues { get; set; }
        public List<IssueDetailed> SprintIssues { get; set; }

        public int InProgressTasksCount { get; set; }
        public int OpenTasksCount { get; set; }
        public int RemainingTasksCount { get; set; }
        public int AdditionalUncompletedTasksCount { get; set; }

        public List<JiraCommit> Commits { get; set; }
        public List<JiraDayLog> DayLogs { get; set; }
        public List<Error> Errors { get; set; }
        public Uri AvatarLink { get; set; }
        public string AvatarId { get; set; }
        public Image Image { get; set; }
        public IndividualDraftInfo IndividualDraftInfo { get; set; }
        public bool IsProjectLead { get; set; }
        public bool IsEmpty { get; set; }
        public bool HasAssignedIssues { get; set; }
        public bool HasDayLogs { get; set; }
        public int MaxHourValue { get; set; }
        public bool HasSprint { get; set; }

        public SummaryGuidelineInfo GuidelineInfo { get; set; }

        public AuthorTasks InProgressTasks { get; set; }
        public AuthorTasks OpenTasks { get; set; }
        public AuthorTasks RemainingTasks { get; set; }

        public Uri IssueSearchUrl { get; set; }

        public JiraAuthor()
        {

        }

        public JiraAuthor(JiraUser user)
        {
            Name = user.displayName;
            EmailAdress = user.emailAddress;
            Username = user.name;
            UserKey = user.key;
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
