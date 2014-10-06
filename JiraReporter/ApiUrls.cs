using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ApiUrls
    {
        public static string Timesheet(string targetGroup, string fromDate, string toDate)
        {
            return string.Format("/rest/timesheet-gadget/1.0/raw-timesheet.xml?targetGroup={0}&startDate={1}&endDate={2}", targetGroup, fromDate, toDate);
        }

        public static string Users(string project)
        {
            return string.Format("/rest/api/2/user/assignable/search?project={0}", project);
        }

        public static string ResolvedIssues(string fromDate, string endDate)
        {
            return string.Format("resolved >= '{0}' & resolved <= '{1}' & statusCategory='Done'", fromDate, endDate);
        }

        public static string IssuesInOpenSprints(string project)
        {
            return string.Format("sprint in openSprints() & project = {0}", project);
        }
    }
}
