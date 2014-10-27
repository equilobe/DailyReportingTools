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
            SetReportDates(timesheet.StartDate, options);

            var timesheetService = new TimesheetService();
            var log = SourceControlProcessor.GetSourceControlLog(policy, options);
            var pullRequests = SourceControlProcessor.GetPullRequests(log);
            timesheetService.SetTimesheetIssues(timesheet, policy, options, pullRequests);
           
            

            return GetReport(timesheet, policy, options, pullRequests);
        }
        private static  Report GetReport(Timesheet timesheet, SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options, List<PullRequest> pullRequests)
        {            
            var sprint = GetSprintReport(policy, options, timesheet, pullRequests);           
            var authors = AuthorsProcessing.GetAuthors(timesheet, sprint, policy, options);
            var report = new Report(policy, options)
            {
                Authors = authors,
                Sprint = sprint,
                PullRequests = pullRequests,
                Date = options.FromDate,
                Summary = new Summary(authors, sprint, pullRequests),
                Title = policy.ReportTitle
            };
                         
            return report;
        }

        private static SprintTasks GetSprintReport(SvnLogReporter.Model.Policy p, SvnLogReporter.Options options, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(p, timesheet, options, pullRequests);
            return report;
        }

        private static void SetReportDates(DateTime referenceDate, SvnLogReporter.Options options)
        {
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(referenceDate);
            options.FromDate = options.FromDate.ToOriginalTimeZone();
            options.ToDate = options.ToDate.ToOriginalTimeZone();
            var dates = new List<DateTime>();
            foreach (var date in options.ReportDates)
                dates.Add(date.ToOriginalTimeZone());
            options.ReportDates = dates;
        }
    }
}
