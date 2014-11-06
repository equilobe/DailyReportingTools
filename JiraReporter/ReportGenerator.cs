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
        public static Report GenerateReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetService = new TimesheetService();
            var timesheetSample = RestApiRequests.GetTimesheet(policy, DateTime.Today.AddDays(1), DateTime.Today.AddDays(1));
            var monthTimesheet = RestApiRequests.GetTimesheet(policy, DateTime.Now.StartOfMonth(), DateTime.Today.AddDays(-1));
            SetReportDates(timesheetSample.StartDate, options);

            var timesheet = RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1));
            var log = SourceControlProcessor.GetSourceControlLog(policy, options);
            var pullRequests = SourceControlProcessor.GetPullRequests(log);
            var commits = SourceControlProcessor.GetCommits(log);
            timesheetService.SetTimesheetIssues(timesheet, policy, options, pullRequests);
                      
            return GetReport(timesheet, monthTimesheet, policy, options, pullRequests, commits);
        }
        private static Report GetReport(Timesheet timesheet, Timesheet monthTimesheet, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests, List<Commit> commits)
        {            
            var sprint = GetSprintReport(policy, options, timesheet, pullRequests);           
            var authors = AuthorsProcessing.GetAuthors(timesheet, sprint, policy, options, commits);
            var report = new Report(policy, options)
            {
                Authors = authors,
                Sprint = sprint,
                PullRequests = pullRequests,
                Date = DateTime.Today,
                Summary = new Summary(authors, sprint, pullRequests, policy, monthTimesheet),
                Title = policy.ReportTitle
            };
                         
            return report;
        }

        private static SprintTasks GetSprintReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            var report = new SprintTasks();
            report.SetSprintTasks(policy, timesheet, options, pullRequests);
            return report;
        }

        private static void SetReportDates(DateTime referenceDate, SourceControlLogReporter.Options options)
        {
            DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(referenceDate);
            //DateTimeExtensions.SetOriginalTimeZoneFromDateAtMidnight(referenceDate, options.FromDate);
            options.FromDate = options.FromDate.ToOriginalTimeZone().Date;
            options.ToDate = options.ToDate.ToOriginalTimeZone().Date;
            var dates = new List<DateTime>();
            foreach (var date in options.ReportDates)
                dates.Add(date.ToOriginalTimeZone().Date);
            options.ReportDates = dates;
        }
    }
}
