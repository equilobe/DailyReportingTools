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

        public static string AllUsers()
        {
            return string.Format("rest/api/2/user/search?maxResults=1000&username=_");
        }

        public static string User(string userName)
        {
            return string.Format("rest/api/latest/user?username={0}", userName);
        }

        public static string Users(string project)
        {
            return string.Format("rest/api/2/user/assignable/search?project={0}&maxResults=1000", project);
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

        public static string UnassignedUncompletedIssues(string projectKey, long sprintId)
        {
            return string.Format("assignee=null and project='{0}' and sprint={1} and statusCategory != 'Done' and issueType != 'sub-task'", projectKey, sprintId);
        }

        public static string AssignedUncompletedIssues(string assignee, string projectKey, long sprintId)
        {
            return string.Format("assignee='{0}' and project='{1}' and sprint={2} and statusCategory != 'Done'", assignee, projectKey, sprintId);
        }

        public static string WorkLogsForUser(string projectKey, string author, string fromDate, string endDate)
        {
            return string.Format("project = {0} AND worklogAuthor = '{1}' AND worklogDate >= '{2}' AND worklogDate <= '{3}'", projectKey, author, fromDate, endDate);
        }

        public static string WorklogsForMultipleUsers(string authors, string startDate)
        {
            return string.Format("worklogAuthor in ({0}) AND worklogDate >= '{1}'", authors, startDate);
        }

        public static string DeletedWorklogs(long since)
        {
            return string.Format("rest/api/2/worklog/deleted?since={0}", since);
        }

        public static string SearchSelectedField(int startAt, string jql)
        {
            return string.Format("rest/api/2/search?startAt={0}&maxResults=100&fields=project,summary,worklog&jql={1}", startAt, jql);
        }

        public static string Board(string projectKey)
        {
            return string.Format("rest/agile/1.0/board?projectKeyOrId={0}", projectKey);
        }

        public static string AllSprints(long boardId, string startAt)
        {
            return string.Format("rest/agile/1.0/board/{0}/sprint?startAt={1}", boardId, startAt);
        }
    }
}
