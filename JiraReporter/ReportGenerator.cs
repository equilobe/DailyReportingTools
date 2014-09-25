using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class ReportGenerator
    {
        public static  Report GenerateReport(SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options)
        {
            var timesheet = RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1));
            var timesheetService = new TimesheetService();
            timesheetService.SetTimesheetIssues(timesheet, policy, options);

            return GetReport(timesheet, policy, options);
        }
        private static  Report GetReport(Timesheet timesheet, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options)
        {
            var sprint = GetSprintReport(policy, options, timesheet);
            var authors = AuthorsProcessing.GetAuthors(timesheet, sprint, policy);
            var report = new Report(policy, options) { Authors = authors, Sprint = sprint, Date = options.FromDate, 
                Summary = new Summary(authors, sprint), Commits = SourceControlProcessor.GetSourceControlLog(policy,options), Title = policy.ReportTitle};
            AuthorsProcessing.SetAuthorsCommits(report);
            return report;
        }

        private static  SprintTasks GetSprintReport(SvnLogReporter.Model.Policy p, SvnLogReporter.Options options, Timesheet timesheet)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(p, timesheet, options);
            return report;
        }
    }
}
