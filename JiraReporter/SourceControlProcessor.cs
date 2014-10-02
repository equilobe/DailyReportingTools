using JiraReporter.Model;
using SvnLogReporter;
using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class SourceControlProcessor
    {
        public static readonly Dictionary<SourceControlType, Func<Policy, Options, ReportBase>> Processors = new Dictionary<SourceControlType, Func<Policy, Options, ReportBase>>()
        {
            {SourceControlType.GitHub, ReportBase.Create<GitHubReport> },
            {SourceControlType.SVN, ReportBase.Create<SvnReport>}
        };

        public static List<Commit> GetSourceControlCommits(Policy policy, Options options)
        {
            var processors = SourceControlProcessor.Processors[policy.SourceControl.Type](policy, options);
            var logs = processors.CreateLog();
            return GetCommits(logs);            
        }

        public static List<Octokit.PullRequest> GetPullRequests(Policy policy, Options options)
        {
                var processor = new GitHubReport(policy, options);
                return processor.GetPullRequests(policy.SourceControl.RepoOwner, policy.SourceControl.RepoName).ToList();
        }

        private static List<Commit> GetCommits(Log log)
        {
            var commits = new List<Commit>();
            foreach (var entry in log.Entries)
            {
                commits.Add(new Commit { Entry = entry });
                commits.Last().Entry.Message = EditMessage(commits.Last().Entry.Message);
            }
            return commits;
        }

        public static string EditMessage(string message)
        {
            return SvnLogReporter.LogProcessor.GetNonEmptyTrimmedLines(message);
        }
    }
}
