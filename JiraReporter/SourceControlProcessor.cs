﻿using Equilobe.DailyReport.Models;
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
            {SourceControlType.BitBucket, ReportBaseSourceControl.Create<BitBucketSourceControl>}
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
            var pullRequests = new List<JiraPullRequest>();
            if (log.PullRequests == null)
                return pullRequests;

            foreach (var pullRequest in log.PullRequests)
                pullRequests.Add();

            return pullRequests;
        }

        public static List<JiraCommit> GetCommits(Log log)
        {
            var commits = new List<JiraCommit>();
            if(log.Entries!=null)
                if(log.Entries.Count>0)
                    foreach (var entry in log.Entries)
                    {
                        commits.Add(new JiraCommit { Entry = entry });
                        commits.Last().Entry.Message = EditMessage(commits.Last().Entry.Message);
                    }
            return commits;
        }

        public static string EditMessage(string message)
        {
            return SourceControlLogReporter.LogProcessor.GetNonEmptyTrimmedLines(message);
        }
    }
}
