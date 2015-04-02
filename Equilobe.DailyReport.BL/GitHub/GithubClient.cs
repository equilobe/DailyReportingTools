using Equilobe.DailyReport.Utils;
using Octokit;
using Octokit.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL.GitHub
{
    public class GithubClient
    {
        ApiConnection Client { get; set; }

        public GithubClient(string username, string password)
        {
            Client = new ApiConnection(new Connection(new ProductHeaderValue("DailyReport")));
            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                Client.Connection.Credentials = new Credentials(username, password);
        }

        public List<GitHubCommit> GetBranchCommits(string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch)
        {
            Ensure.ArgumentNotNullOrEmptyString(repositoryOwner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(repositoryName, "name");
            return Client.GetAll<GitHubCommit>(ApiUrls.RepositoryCommitsBranchDate(repositoryOwner, repositoryName, sinceDate, untilDate, branch)).Result.ToList();
        }

        public List<GitHubCommit> GetAllCommits(string owner, string name, string sinceDate, string untilDate)
        {
            var branches = GetBranches(owner, name);
            var commits = branches.SelectMany(br => GetBranchCommits(owner, name, sinceDate, untilDate, br.Name)).ToList();
            commits.ForEach(AddName);

            return commits.ToLookup(c => c.Sha).Select(commit => commit.First()).ToList();
        }

        public List<Branch> GetBranches(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            return Client.GetAll<Branch>(ApiUrls.RepoBranches(owner, name)).Result.ToList();
        }

        public List<PullRequest> GetPullRequests(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            var pullRequests = Client.GetAll<PullRequest>(ApiUrls.PullRequests(owner, name)).Result.ToList();
            pullRequests.ForEach(p => p.User.Name = GetUser(p.User.Login).Name);

            return pullRequests;
        }

        public List<Octokit.User> GetAllContributors(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            return Client.GetAll<Octokit.User>(ApiUrls.RepositoryContributors(owner, name)).Result.ToList();
        }

        public Octokit.User GetUser(string username)
        {
            Ensure.ArgumentNotNullOrEmptyString(username, "username");
            return Client.Get<Octokit.User>(ApiUrls.User(username)).Result;
        }

        static bool HasAuthor(GitHubCommit commit)
        {
            if (commit.Author != null)
                return true;

            return false;
        }

        bool HasName(GitHubCommit commit)
        {
            if (commit.Commit.Author.Name == GetUser(commit.Author.Login).Name)
                return true;

            return false;
        }

        void AddName(GitHubCommit commit)
        {
            if (HasAuthor(commit) && !HasName(commit))
                commit.Commit.Author.Name = GetUser(commit.Author.Login).Name;
        }

    }
}
