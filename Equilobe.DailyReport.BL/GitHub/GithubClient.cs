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
        ApiConnection Connection { get; set; }

        public GithubClient(string username, string password)
        {
            Connection = new ApiConnection(new Connection(new ProductHeaderValue("Eq"), new InMemoryCredentialStore(new Credentials(username, password))));
        }

        public List<GitHubCommit> GetBranchCommits(string repositoryOwner, string repositoryName, string sinceDate, string untilDate, string branch)
        {
            Ensure.ArgumentNotNullOrEmptyString(repositoryOwner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(repositoryName, "name");
            return Connection.GetAll<GitHubCommit>(ApiUrls.RepositoryCommitsBranchDate(repositoryOwner, repositoryName, sinceDate, untilDate, branch)).Result.ToList();
        }

        public List<GitHubCommit> GetAllCommits(string owner, string name, string sinceDate, string untilDate)
        {
            var branches = GetBranches(owner, name);
            var commits = branches.SelectMany(br => GetBranchCommits(owner, name, sinceDate, untilDate, br.Name)).ToList();
            commits.ForEach(AddName);

            return commits.Distinct().ToList();
        }

        public List<Branch> GetBranches(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            return Connection.GetAll<Branch>(ApiUrls.RepoBranches(owner, name)).Result.ToList();
        }

        public List<PullRequest> GetPullRequests(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            var pullRequests = Connection.GetAll<PullRequest>(ApiUrls.PullRequests(owner, name)).Result.ToList();
            pullRequests.ForEach(p => p.User.Name = GetUser(p.User.Login).Name);

            return pullRequests;
        }

        public List<Octokit.User> GetAllContributors(string owner, string name)
        {
            Ensure.ArgumentNotNullOrEmptyString(owner, "owner");
            Ensure.ArgumentNotNullOrEmptyString(name, "name");
            return Connection.GetAll<Octokit.User>(ApiUrls.RepositoryContributors(owner, name)).Result.ToList();
        }

        public Octokit.User GetUser(string username)
        {
            Ensure.ArgumentNotNullOrEmptyString(username, "username");
            return Connection.Get<Octokit.User>(ApiUrls.User(username)).Result;
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
