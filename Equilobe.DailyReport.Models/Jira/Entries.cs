using System;
using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Entry
    {
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "timeSpent")]
        public int TimeSpent { get; set; }

        [DataMember(Name = "author")]
        public string Author { get; set; }

        [DataMember(Name = "authorFullName")]
        public string AuthorFullName { get; set; }

        [DataMember(Name = "created")]
        public string Created { get; set; }

        [DataMember(Name = "startDate")]
        public string StartDate { get; set; }

        [DataMember(Name = "updateAuthor")]
        public string UpdateAuthor { get; set; }

        [DataMember(Name = "updateAuthorFullName")]
        public string UpdateAuthorFullName { get; set; }

        [DataMember(Name = "updated")]
        public string Updated { get; set; }

        public DateTime CreatedAt
        {
            get
            {
                return Convert.ToDateTime(Created);
            }
            set
            {

            }
        }

        public DateTime StartedAt
        {
            get
            {
                return Convert.ToDateTime(StartDate);
            }
            set
            {

            }
        }

        public DateTime UpdatedAt
        {
            get
            {
                return Convert.ToDateTime(Updated);
            }
            set
            {

            }
        }
    }
}
