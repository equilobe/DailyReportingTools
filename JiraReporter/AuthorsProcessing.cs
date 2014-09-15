using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter
{
    class AuthorsProcessing
    {
        private static Dictionary<string, List<Issue>> GetAuthorsDict(Timesheet report)
        {
            var authors = new Dictionary<string, List<Issue>>();
            foreach (var issue in report.Worklog.Issues)
                Add(authors, issue.Entries.First().AuthorFullName, issue);
            return authors;

        }

        private static void Add(Dictionary<string, List<Issue>> dict, string key, Issue value)
        {
            if (dict.ContainsKey(key))
            {
                List<Issue> list = dict[key];
                list.Add(value);
            }
            else
            {
                List<Issue> list = new List<Issue>();
                list.Add(value);
                dict.Add(key, list);
            }
        }

        public static List<Author> GetAuthors(Timesheet timesheet)
        {
            var authors = GetAuthorsDict(timesheet);
            var authorsNew = new List<Author>();
            foreach (var author in authors)
                authorsNew.Add(new Author { Name = author.Key, Issues = author.Value });
            authorsNew = OrderAuthorsIssues(authorsNew);
            return authorsNew;
        }

        public static void SetAuthorsTimeSpent(List<Author> authors)
        {
            foreach (var author in authors)
                SetAuthorTimeSpent(author);
        }

        private static void SetAuthorTimeSpent(Author author)
        {
            foreach (var issue in author.Issues)
                author.TimeSpent += issue.TimeSpent;

            author.TimeLogged = author.TimeSpent.ToString();
        }

        public static void SetAuthorsTimeFormat(List<Author> authors)
        {
            foreach (var author in authors)
                author.TimeLogged = TimeFormatting.SetTimeFormat(author.TimeSpent);
        }

        public static List<Author> OrderAuthorsName(List<Author> authors)
        {
            return authors.OrderBy(a => a.Name).ToList();
        }

        public static List<Author> OrderAuthorsTime(List<Author> authors)
        {
            return authors.OrderByDescending(a => a.TimeSpent).ToList();
        }

        public static List<Author> OrderAuthorsIssues(List<Author> authors)
        {
            foreach (var author in authors)
                author.Issues = Issue.OrderIssues(author.Issues);
            return authors;
        }
    }
}
