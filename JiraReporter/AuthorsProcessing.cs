using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    class AuthorsProcessing
    {
        public static List<Author> GetAuthors(Timesheet timesheet, SprintStatus report)
        {
            var authors = GetAuthorsDict(timesheet);
            var authorsNew = new List<Author>();
            foreach (var author in authors)
            {
                authorsNew.Add(new Author { Name = author.Key, Issues = author.Value });
                SetUnfinishedTasks(report, authorsNew.Last());
            }
            SetAuthors(authorsNew, report);
            
            return authorsNew;
        }

        private static Dictionary<string, List<Issue>> GetAuthorsDict(Timesheet timesheet)
        {
            var authors = new Dictionary<string, List<Issue>>();
            foreach (var issue in timesheet.Worklog.Issues)
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

        private static void SetAuthors(List<Author> authors, SprintStatus report)
        {
            authors = OrderAuthorsIssues(authors);
            SetAuthorsTimeSpent(authors);
            SetAuthorsTimeFormat(authors);
            OrderAuthorsName(authors); 
               
        }

        private static void SetAuthorsTimeSpent(List<Author> authors)
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

        private static void OrderAuthorsName(List<Author> authors)
        {
            authors = authors.OrderBy(a => a.Name).ToList();
        }

        private static List<Author> OrderAuthorsTime(List<Author> authors)
        {
            return authors.OrderByDescending(a => a.TimeSpent).ToList();
        }

        private static List<Author> OrderAuthorsIssues(List<Author> authors)
        {
            foreach (var author in authors)
                author.Issues = Issue.OrderIssues(author.Issues);
            return authors;
        }

        private static void SetUnfinishedTasks(SprintStatus report, Author author)
        {
            author.InProgressTasks = GetTasks(report.InProgressTasks, author);
            if(author.InProgressTasks!=null)
                author.InProgressTasksCount = author.InProgressTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
            author.InProgressTasksTimeLeftSeconds = TasksService.GetTasksTimeLeftSeconds(author.InProgressTasks);
            author.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.InProgressTasksTimeLeftSeconds);

            author.OpenTasks = GetTasks(report.OpenTasks, author);
            if(author.OpenTasks!=null)
                author.OpenTasksCount = author.OpenTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
            author.InProgressTasksTimeLeftSeconds = TasksService.GetTasksTimeLeftSeconds(author.InProgressTasks);
            author.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(author.InProgressTasksTimeLeftSeconds);
        }

        private static List<Task> GetTasks(List<Task> tasks, Author author)
        {
            var unfinishedTasks = new List<Task>();
            foreach (var task in tasks)
                if (task.Issue.Assignee == author.Name)
                    unfinishedTasks.Add(task);
            return unfinishedTasks;
        }        
    }
}
