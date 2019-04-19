using System.Runtime.Serialization;

namespace Equilobe.DailyReport.Models.Jira
{
    [DataContract]
    public class Entry : Comment
    {
        [DataMember(Name = "comment")]
        public string Comment { get; set; }

        [DataMember(Name = "timeSpent")]
        public int TimeSpent { get; set; }

        [DataMember(Name = "authorFullName")]
        public string AuthorFullName { get; set; }

        [DataMember(Name = "updateAuthorFullName")]
        public string UpdateAuthorFullName { get; set; }
    }
}
