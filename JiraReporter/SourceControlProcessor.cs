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

        public static SvnLogReporter.Model.Log GetSourceControlLog(Policy policy, Options options)
        {
            var processors = SourceControlProcessor.Processors[policy.SourceControl.Type](policy, options);
            var log = processors.CreateLog();
            return log;
        }

        public static List<PullRequest> GetPullRequests(Log log)
        {
            var pullRequests = new List<PullRequest>();
            if(log.PullRequests!=null)
                if(log.PullRequests.Count>0)
                    foreach (var pullRequest in log.PullRequests)
                        pullRequests.Add(new PullRequest { GithubPullRequest = pullRequest });
            return pullRequests;
        }

        public static List<Commit> GetCommits(Log log)
        {
            var commits = new List<Commit>();
            if(log.Entries!=null)
                if(log.Entries.Count>0)
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
