using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL
{
    public class JiraApiUrls
    {
        public static string Project(long id)
        {
            return string.Format("rest/api/2/project/{0}", id);
        }

        public static string Projects()
        {
            return "rest/api/2/project";
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

        public static string AllSprints(string rapidViewId, string projectKey)
        {
            return string.Format("rest/greenhopper/1.0/sprintquery/{0}?jql=project='{1}?includeHistoricSprints=true&includeFutureSprints=true", rapidViewId, projectKey);
        }

        public static string Issue(string key)
        {
            return string.Format("rest/api/2/issue/{0}", key);
        }

        public static string IssueWorklogs(string key)
        {
            return string.Format("rest/api/2/issue/{0}/worklog", key);
        }

        public static string Search(string jql)
        {
            return string.Format("rest/api/2/search?jql={0}&startAt=0&maxResults=1000", jql);
        }

        public static string ResolvedIssues(string projectKey, string fromDate, string endDate)
        {
            return string.Format("statusCategory = 'Done' AND resolved >= '{0}' AND resolved <= '{1}' AND project='{2}'", fromDate, endDate, projectKey);
        }

        public static string IssueInCurrentSprint(string project, string sprintId)
        {
            return string.Format("project = '{0}' AND sprint = {1}", project, sprintId);
        }

        public static string IssuesInOpenSprints(string project)
        {
            return string.Format("project = '{0}' AND sprint in openSprints()", project);
        }

        public static string UnassignedUncompletedIssues(string projectKey, int sprintId)
        {
            return string.Format("assignee=null and project='{0}' and sprint={1} and statusCategory != 'Done' and issueType != 'sub-task'", projectKey, sprintId);
        }

        public static string AssignedUncompletedIssues(string assignee, string projectKey, int sprintId)
        {
            return string.Format("assignee='{0}' and project='{1}' and sprint={2} and statusCategory != 'Done'", assignee, projectKey, sprintId);
        }

        public static string WorkLogs(string projectKey, string author, string fromDate, string endDate)
        {
            return string.Format("project = {0} AND worklogAuthor = '{1}' AND worklogDate >= '{2}' AND worklogDate <= '{3}'", projectKey, author, fromDate, endDate);
        }

        public static string Board(string projectKey)
        {
            return string.Format("rest/agile/1.0/board?projectKeyOrId={0}", projectKey);
        }

        public static string AllSprints(string boardId)
        {
            return string.Format("rest/agile/1.0/board/{0}/sprint", boardId);
        }
    }
}
