using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Author
    {
        public string Name { get; set; }
        public List<Issue> Issues { get; set; }
        public string TimeLogged { get; set; }

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
           return authorsNew;
       }
   
        public static void SetAuthorsTimeSpent(List<Author> authors)
        {
            foreach (var author in authors)
                SetAuthorTimeSpent(author);
        }

        private static void SetAuthorTimeSpent(Author author)
        {
            int totalTime = 0;
            foreach (var issue in author.Issues)
            {
                var time = Convert.ToInt32(issue.TimeSpent);
                totalTime += time;
            }
            author.TimeLogged = totalTime.ToString();
        }

        public static void SetAuthorsTimeFormat(List<Author> authors)
        {
            foreach (var auhtor in authors)
                SetAuthorTimeFormat(auhtor);
        }

        private static void SetAuthorTimeFormat(Author author)
        {
            int time;
            time = Convert.ToInt32(author.TimeLogged);
            author.TimeLogged = Timesheet.SetTimeFormat(time);
        }

        public static void SetIssuesTime(List<Author> authors)
        {
            foreach (var author in authors)
                SetIssueTime(author);
        }

        public static void SetIssueTime(Author author)
        {
            foreach (var issue in author.Issues)
                Issue.SetIssueTimeFormat(issue);
        }
    }
}
