using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public class ApiUrls
    {
        public static string Project(string id)
        {
            return string.Format("rest/api/2/project/{0}", id);
        }
        public static string Timesheet(string targetUser, string fromDate, string toDate)
        {
            return string.Format("rest/timesheet-gadget/1.0/raw-timesheet.xml?targetUser={0}&startDate={1}&endDate={2}", targetUser, fromDate, toDate);
        }

        public static string User(string userName)
        {
            return string.Format("rest/api/latest/user?username={0}", userName);
        }

        public static string Users(string project)
        {
            return string.Format("rest/api/2/user/assignable/search?project={0}", project);
        }

        public static string RapidView(string id)
        {
            return string.Format("rest/greenhopper/1.0/xboard/work/allData/?rapidViewId={0}", id);
        }

        public static string RapidViews()
        {
            return "rest/greenhopper/1.0/rapidviews/list";
        }

        public static string Sprint(string rapidViewId, string sprintId)
        {
            return string.Format("rest/greenhopper/1.0/rapid/charts/sprintreport?rapidViewId={0}&sprintId={1}", rapidViewId, sprintId);
        }

        public static string AllSprints(string rapidViewId)
        {
            return string.Format("rest/greenhopper/1.0/sprintquery/{0}", rapidViewId);
        }
        public static string IssueByKey(string issueKey)
        {
            return string.Format("rest/api/2/issue/{0}", issueKey);
        }

        public static string Issue()
        {
            return "rest/api/2/issue";
        }

        public static string Priority()
        {
            return "rest/api/2/priority";
        }

        public static string CreateMeta()
        {
            return "rest/api/2/issue/createmeta";
        }

        public static string Status()
        {
            return "rest/api/2/status";
        }

        public static string ApplicationProperties()
        {
            return "rest/api/2/application-properties";
        }

        public static string AttachmentById(string attachmentId)
        {
            return string.Format("rest/api/2/attachment/{0}", attachmentId);
        }

        public static string Project()
        {
            return "rest/api/2/project";
        }

        public static string Search(string jql)
        {
            return string.Format("rest/api/2/search?jql={0}", jql);
        }

        public static string ResolvedIssues(string fromDate, string endDate)
        {
            return string.Format("statusCategory = 'Done' AND resolved >= '{0}' AND resolved <= '{1}'", fromDate, endDate);
        }

        public static string IssuesInOpenSprints(string project)
        {
            return string.Format("project = '{0}' AND sprint in openSprints()", project);
        }
    }
}
