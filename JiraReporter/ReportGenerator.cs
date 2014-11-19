using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    public enum TimesheetType { ReportTimesheet, MonthTimesheet, SprintTimesheet};

    class ReportGenerator
    {
        public static Report GenerateReport(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetService = new TimesheetService();

            var log = SourceControlProcessor.GetSourceControlLog(policy, options);
            var pullRequests = SourceControlProcessor.GetPullRequests(log);
            var commits = SourceControlProcessor.GetCommits(log);

            var timesheetCollection = GenerateReportTimesheets(policy, options);

            timesheetService.SetTimesheetCollection(timesheetCollection, policy, options, pullRequests);

            return GetReport(timesheetCollection, policy, options, pullRequests, commits);
        }
        private static Report GetReport(Dictionary<TimesheetType, Timesheet> timesheetCollection, SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests, List<Commit> commits)
        {
            var sprint = GetSprintReport(policy, options, timesheetCollection[TimesheetType.ReportTimesheet], pullRequests);           
            var authors = AuthorsProcessing.GetAuthors(timesheetCollection, sprint, policy, options, commits);
            var report = new Report(policy, options)
            {
                Authors = authors,
                SprintTasks = sprint,
                PullRequests = pullRequests,
                Date = DateTime.Now.ToOriginalTimeZone().Date,
                Summary = new Summary(authors, sprint, pullRequests, policy, timesheetCollection)
                {
                    FromDate = options.FromDate,
                    ToDate = options.ToDate
                },
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

        private static Dictionary<TimesheetType, Timesheet> GenerateReportTimesheets(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options)
        {
            var timesheetDictionary = new Dictionary<TimesheetType, Timesheet>();
            var sprint = GenerateSprint(policy);
            timesheetDictionary.Add(TimesheetType.ReportTimesheet, RestApiRequests.GetTimesheet(policy, options.FromDate, options.ToDate.AddDays(-1)));
            timesheetDictionary.Add(TimesheetType.MonthTimesheet, RestApiRequests.GetTimesheet(policy, DateTime.Now.ToOriginalTimeZone().StartOfMonth(), DateTime.Now.ToOriginalTimeZone().AddDays(-1)));
            timesheetDictionary.Add(TimesheetType.SprintTimesheet, RestApiRequests.GetTimesheet(policy, sprint.StartDate.ToOriginalTimeZone(), sprint.EndDate.ToOriginalTimeZone()));
            return timesheetDictionary;
        }

        private static Sprint GenerateSprint(SourceControlLogReporter.Model.Policy policy)
        {
            var jira = new JiraService(policy);
            return jira.GetLatestSprint();
        }
    }
}
