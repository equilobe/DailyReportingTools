﻿using Octokit;
using Octokit.Internal;
using RazorEngine;
using SvnLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvnLogReporter
{
    public class GitHubReport : ReportBase
    {
        public GitHubReport(Policy p, Options o):base(p,o)
        {

        }

        public override Log CreateLog()
        {
            var pullRequests = GetPullRequests(Policy.SourceControl.RepoOwner, Policy.SourceControl.RepoName).ToList();
            var commits = GetReportCommits();
            return LoadLog(commits, pullRequests);
        }

        protected Log LoadLog(List<GitHubCommit> commits, List<PullRequest> pullRequests)
        {
            var log = new Log();
            if (pullRequests != null)
            {
                log.PullRequests = pullRequests;
                SetPullRequestsAuthors(pullRequests);
            }
            log.Entries = new List<LogEntry>();
            var find = new List<PullRequest>();
            foreach (var commit in commits)
            {
                log.Entries.Add(
                 new LogEntry
                 {
                     Author = commit.Commit.Author.Name,
                     Date = commit.Commit.Author.Date,
                     Message = commit.Commit.Message,
                     Revision = commit.Sha,
                     Link = commit.HtmlUrl,
                     PullRequests = pullRequests.FindAll(p => p.User.Login == commit.Author.Login)
                 });                
            }

            log.RemoveWrongEntries(Options.FromDate);
            return log;
        }

        private void SetPullRequestsAuthors(List<PullRequest> pullRequests)
        {
            if (pullRequests != null)
                if (pullRequests.Count > 0)
                    foreach (var pullRequest in pullRequests)
                        pullRequest.User.Name = GetUserInfo(pullRequest.User.Login).Name;
        }

        protected override List<Report> GetReports(Log log)
        {
            var reports = new List<Report>();
            var logs = GetDayLogs(log);
            var report = new Report();
            var dates = new List<DateTime>();
            Options.GetDates(dates);
            reports = EmptyReports(logs, dates);
            foreach (var logDict in logs)
            {

                report = LogProcessor.GetReport(logDict.Value);
                report.ReportDate = logDict.Key;
                reports.Add(report);
            }
            reports = reports.OrderBy(r => r.ReportDate).ToList();
            reports.First().PullRequests = log.PullRequests;
            return reports;
        }
     
        protected IReadOnlyList<GitHubCommit> GetAllCommits(string owner, string name, string sinceDate, string untilDate, string branch)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControl.Username, Policy.SourceControl.Password))));
            return connectionAll.GetAll<GitHubCommit>(ApiUrls.RepositoryCommitsBranchDate(owner, name, sinceDate, untilDate, branch)).Result;
        }

        protected IReadOnlyList<Branch> GetBranches(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControl.Username, Policy.SourceControl.Password))));
            return connectionAll.GetAll<Branch>(ApiUrls.RepoBranches(owner, name)).Result;
        }

        public IReadOnlyList<PullRequest> GetPullRequests(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControl.Username, Policy.SourceControl.Password))));
            return connectionAll.GetAll<PullRequest>(ApiUrls.PullRequests(owner,name)).Result;
        }

        protected List<GitHubCommit> ConcatCommits(string owner, string name, string sinceDate, string untilDate)
        {
            var branches = GetBranches(owner, name);
            var commits = new List<GitHubCommit>();
            var newCommits = new List<GitHubCommit>(); ;
            foreach (var branch in branches)
                commits = commits.Concat(GetAllCommits(owner, name, sinceDate, untilDate, branch.Name)).ToList();
            return commits;
        }

        public static List<GitHubCommit> RemoveDuplicateCommits(List<GitHubCommit> commits)
        {
            commits = commits.ToLookup(t => t.Sha).Select(o => o.First()).ToList();
            return commits;
        }

        public IReadOnlyList<Octokit.User> GetAllContributors(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            ApiConnection connectionAll = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControl.Username, Policy.SourceControl.Password))));
            return connectionAll.GetAll<Octokit.User>(ApiUrls.RepositoryContributors(owner, name)).Result;
        }

        public Octokit.User GetUserInfo(string username)
        {
            Ensure.ArgumentNotNullOrEmptyString(username, "username");
            ApiConnection connection = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(Policy.SourceControl.Username, Policy.SourceControl.Password))));
            return connection.Get<Octokit.User>(ApiUrls.User(username)).Result;
        }

        public static bool HasAuthor(GitHubCommit commit)
        {
            if (commit.Author != null)
                return true;

            return false;
        }

        protected bool HasName(GitHubCommit commit)
        {
            if (commit.Commit.Author.Name == GetUserInfo(commit.Author.Login).Name)
                return true;

            return false;
        }

        protected void AddName(List<GitHubCommit> commits)
        {
            foreach (var commit in commits)
                if (HasAuthor(commit) && !HasName(commit))
                    commit.Commit.Author.Name = GetUserInfo(commit.Author.Login).Name;
        }
        protected List<GitHubCommit> GetReportCommits()
        {
            string fromDate = Options.DateToISO(Options.FromDate);
            string toDate = Options.DateToISO(Options.ToDate);
            var commits = ConcatCommits(Policy.SourceControl.RepoOwner, Policy.SourceControl.RepoName, fromDate, toDate);
            commits = RemoveDuplicateCommits(commits);

            AddName(commits);

            return commits;
        } 
    }
}
