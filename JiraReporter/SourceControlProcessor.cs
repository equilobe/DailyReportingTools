using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.Models.Storage;
using JiraReporter.Model;
using JiraReporter.SourceControl;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class SourceControlProcessor
    {
        public static readonly Dictionary<SourceControlType, Func<JiraReport, ReportBase>> Processors = new Dictionary<SourceControlType, Func<JiraReport, ReportBase>>()
        {
            {SourceControlType.GitHub, ReportBaseSourceControl.Create<GitHubReportSourceControl>},
            {SourceControlType.SVN, ReportBaseSourceControl.Create<SvnReportSourceControl>},
            {SourceControlType.Bitbucket, ReportBaseSourceControl.Create<BitBucketSourceControl>}
        };

        public static Log GetSourceControlLog(JiraReport report)
        {
            if (report.Policy.SourceControlOptions.Type == SourceControlType.None)
                return new Log();

            var processor = SourceControlProcessor.Processors[report.Policy.SourceControlOptions.Type](report);
            var log = processor.CreateLog();
            return log;
        }

        public static List<JiraPullRequest> GetPullRequests(Log log)
        {
            if (log.PullRequests == null)
                return new List<JiraPullRequest>();

            return log.PullRequests.Select(pr => new JiraPullRequest { GithubPullRequest = pr }).ToList();
        }

        public static List<JiraCommit> GetCommits(Log log)
        {
            if (log.Entries == null)
                return new List<JiraCommit>();

            return log.Entries.Select(e => new JiraCommit { Entry = GetEntryWithTrimmedMessage(e) }).ToList();      
        }

        public static LogEntry GetEntryWithTrimmedMessage(LogEntry entry)
        {
            entry.Message = GetTrimmedMessage(entry.Message);

            return entry;
        }

        public static string GetTrimmedMessage(string message)
        {
            return SourceControlLogReporter.LogProcessor.GetNonEmptyTrimmedLines(message);
        }
    }
}
